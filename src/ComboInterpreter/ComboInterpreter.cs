using Slippi.NET;
using Slippi.NET.Melee.Types;
using Slippi.NET.Stats;
using Slippi.NET.Stats.Types;
using Slippi.NET.Types;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace ComboInterpreter;

public abstract class BaseComboInterpreter : IDisposable
{
    protected const bool LOG_VERBOSE = true;

    protected readonly bool _isReplay;
    protected readonly SlippiGame _game;

    protected readonly StatsComputer? _statsComputer;        // replay
    protected readonly Dictionary<int, FrameEntry>? _frames; // replay

    protected readonly ActionsComputer _actionsComputer;
    protected readonly DIComputer _diComputer;
    protected readonly int _playerIndex;

    protected readonly TaskCompletionSource _gameEnd;

    protected List<ActionEvent> _eventBuffer = [];
    protected List<PendingAction> _pendingBuffer = [];
    protected BlockingCollection<InterpretedCombo> _combos = new BlockingCollection<InterpretedCombo>();

    public BaseComboInterpreter(Character character, bool isReplay, int startFrame, string gamePath, params string[] netplayCodesOrNames)
    {
        _isReplay = isReplay;
        _actionsComputer = new ActionsComputer();
        _diComputer = new DIComputer();

        _game = new SlippiGame(gamePath, new StatOptions()
        {
            ProcessOnTheFly = !isReplay,
            FirstFrame = isReplay ? startFrame : (int)Frames.FIRST
        }, customComputers: isReplay ? null : [_actionsComputer, _diComputer]);
        _gameEnd = new TaskCompletionSource();

        if (!isReplay)
        {
            _game.OnGameEnd += OnGameEnd;

            if (_game.GetGameEnd() is not null)
            {
                _gameEnd.SetResult();
            }
        }
        else
        {
            _statsComputer = new StatsComputer(new StatOptions() { ProcessOnTheFly = false, FirstFrame = startFrame });
            _statsComputer.Register(_actionsComputer, _diComputer);
            _statsComputer.Setup(_game.GetSettings() ?? throw new Exception("Invalid replay"));

            _frames = _game.GetFrames();
        }

        int? candidatePlayerIndex = null;
        candidatePlayerIndex = _game.GetSettings()?.Players
                .Where(p => netplayCodesOrNames.Any(c => string.Equals(c, p.ConnectCode, StringComparison.Ordinal) ||
                                                         string.Equals(c, p.DisplayName, StringComparison.Ordinal)))
                .FirstOrDefault()?.PlayerIndex;

        if (candidatePlayerIndex is null)
        {
            candidatePlayerIndex = _game.GetSettings()?.Players
                .Where(p => p.Character == character)
                .FirstOrDefault()?.PlayerIndex;
        }

        if (candidatePlayerIndex is null)
        {
            throw new Exception(
                $"Failed to find a match. \n" +
                $"Searched for: {string.Join(",", [.. netplayCodesOrNames])}\n" +
                $"Found: {string.Join(",", _game.GetMetadata()?.Players.Select(p => $"{p.Value.Names?.Netplay ?? "N/A"} / {p.Value.Names?.Code ?? "N/A"}") ?? [])}"
            );
        }
        else
        {
            _playerIndex = candidatePlayerIndex.Value;
            Debug.WriteLine($"Player index: {_playerIndex}");
        }

        _actionsComputer.OnAction += OnAction;
        _actionsComputer.OnRawAction += OnRawAction;
        _diComputer.OnDI += HandleDI;

        _game.GetStats();

    }

    public BlockingCollection<InterpretedCombo> ComboStream => _combos;

    public event EventHandler<DIEventArgs>? OnDI;

    public async Task WaitForLiveGameEndAsync()
    {
        if (_isReplay)
        {
            // nothing to do
        }
        else
        {
            while (!_gameEnd.Task.IsCompleted)
            {
                _game?.GetStats(); // more or less to ensure the frame updates are pumped
                await Task.Delay(20);
            }
        }
    }

    public void ProcessFrame(int frame)
    {
        if (_isReplay && _frames is not null)
        {
            _statsComputer?.AddFrame(_frames[frame]);
            _statsComputer?.Process();
        }
        else
        {
            // if live, the file should be live processed by _game, not manually
        }
    }

    protected virtual void OnAction(object? sender, ActionEventArgs args)
    {
        ActionEvent? actionEvent = PreProcessAction(args);
        if (actionEvent is not null)
        {
            HandleActionEvent(actionEvent);

            PostProcessActionEvent(actionEvent);
        }
    }

    protected virtual void HandleDI(object? sender, DIEventArgs args)
    {
        if (args.PlayerIndex != _playerIndex)
        {
            SimpleButtons stickDirection = Utils.GetStick(Utils.ToSimpleButtons(args.PreFrameUpdate));
            OnDI?.Invoke(sender, args);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected virtual ActionEvent? PreProcessAction(ActionEventArgs args)
    {
        if (args.Action == Actions.None)
        {
            return null;
        }

        int frame = args.Frame.Frame!.Value;
        if (args.PlayerIndex == _playerIndex)
        {
            ActionEvent actionEvent = new ActionEvent()
            {
                Action = args.Action,
                FrameEntry = args.Frame,
            };

            _eventBuffer.Add(actionEvent);
            if (LOG_VERBOSE)
            {
                Debug.WriteLine($"VERBOSE: {actionEvent.Action.ToString()}");
            }

            if (!_isReplay)
            {
                ProcessPendingActions(actionEvent);
            }

            return actionEvent;
        }

        return null;
    }

    protected virtual void HandleActionEvent(ActionEvent actionEvent)
    {
        switch (actionEvent.Action)
        {
            case Actions.AirDodge:
                {
                    _pendingBuffer.Add(new PendingAction()
                    {
                        Action = actionEvent,
                        CancelIf = static c => c.Action == Actions.Wavedash || c.Action == Actions.Waveland,
                        FramesLeft = 8,
                    });

                    break;
                }
            case Actions.Dash:
                {
                    _pendingBuffer.Add(new PendingAction()
                    {
                        Action = actionEvent,
                        ActionsLeft = 1,
                        CancelIf = static (c) => c.Action == Actions.DashDance || c.Action == Actions.Dash
                    });

                    break;
                }
            case Actions.PlatformDrop:
                {
                    if (IsFallThroughShieldDrop())
                    {
                        _pendingBuffer.Add(new PendingAction()
                        {
                            Action = new ActionEvent()
                            {
                                Action = Actions.ShieldDrop,
                                FrameEntry = actionEvent.FrameEntry,
                                HasContinuation = false,
                            },
                            ActionsLeft = 1,
                            ContinuationIf = static (c) => c.Action == Actions.Bair ||
                                                           c.Action == Actions.Nair ||
                                                           c.Action == Actions.DAir ||
                                                           c.Action == Actions.Fair ||
                                                           c.Action == Actions.UAir
                        });
                    }
                    else
                    {
                        InterpretActionEvent(actionEvent);
                    }

                    break;
                }
            case Actions.Shield:
                {
                    _pendingBuffer.Add(new PendingAction()
                    {
                        Action = actionEvent,
                        ActionsLeft = 1,
                        CancelIf = static (c) => c.Action == Actions.PlatformDrop
                    });

                    break;
                }
            case Actions.JumpCancel: // can override in derived class for e.g. fox jc upsmash
                {
                    _pendingBuffer.Add(new PendingAction()
                    {
                        Action = actionEvent,
                        ActionsLeft = 1,
                        ContinuationIf = static (c) => c.Action == Actions.Grab,
                        CancelIf = static (c) => (c.Action != Actions.USmash && c.Action != Actions.Grab) || c.Action == Actions.JumpCancel,
                    });

                    break;
                }
            case Actions.FastFall:
            case Actions.WallJump:
            case Actions.Jab:
            case Actions.Grab:
            case Actions.Nair:
            case Actions.UAir:
            case Actions.Roll:
            case Actions.Tech:
            case Actions.Bair:
            case Actions.DAir:
            case Actions.Fair:
            case Actions.BThrow:
            case Actions.UThrow:
            case Actions.DThrow:
            case Actions.SpotDodge:
            case Actions.Wavedash:
            case Actions.Waveland:
            case Actions.DashAttack:
            case Actions.UTilt:
            case Actions.DTilt:
            case Actions.FTilt:
            case Actions.DashDance:
            case Actions.LCancel:
            case Actions.FSmash:
            case Actions.USmash:
            case Actions.DSmash:
                {
                    InterpretActionEvent(actionEvent);
                    break;
                }
            default:
                if (actionEvent.Action != Actions.None)
                {
                    if (LOG_VERBOSE)
                    {
                        Console.Write($" Skip: {actionEvent.Action.ToString()} ");
                    }
                }

                break;
        }
    }

    protected virtual void PostProcessActionEvent(ActionEvent actionEvent)
    {
        if (_isReplay)
        {
            ProcessPendingActions(actionEvent);
        }
    }

    protected virtual void OnRawAction(object? sender, RawActionEventArgs actionState)
    {
    }

    protected virtual void OnGameEnd(object? sender, EventArgs args)
    {
        _gameEnd.SetResult();
    }

    protected void ProcessPendingActions(ActionEvent currentEvent)
    {
        if (_isReplay && _frames is not null)
        {
            // we're not live, so we can just look ahead in time
            if (_pendingBuffer.Count > 0)
            {
                if (_pendingBuffer.Count != 1)
                {
                    throw new Exception("Should only have to process one pending event");
                }

                PendingAction pendingAction = _pendingBuffer[0];
                _pendingBuffer.Clear();

                ActionsComputer futureComputer = new ActionsComputer();
                futureComputer.Setup(_game.GetSettings()!);

                int actionsLeft = pendingAction.ActionsLeft;
                int framesLeft = pendingAction.FramesLeft;
                ActionEvent? futureAction = null;
                 
                void OnFutureAction(object? sender, ActionEventArgs args)
                {
                    if (args.PlayerIndex == _playerIndex)
                    {
                        if (actionsLeft != -1)
                        {
                            actionsLeft--;
                        }

                        futureAction = new ActionEvent() { Action = args.Action, FrameEntry = args.Frame };
                    }
                }

                void OnFutureRawAction(object? sender, RawActionEventArgs args)
                {
                    if (args.PlayerIndex == _playerIndex)
                    {
                        Actions action = ComputeActionFromActionState(args.ActionState);
                        if (action != Actions.None)
                        {
                            OnFutureAction(null, new ActionEventArgs() { Action = action, Frame = args.Frame, PlayerIndex = args.PlayerIndex });
                        }
                    }
                }

                futureComputer.OnRawAction += OnFutureRawAction;
                futureComputer.OnAction += OnFutureAction;

                int currentFrame = pendingAction.Action.Frame + 1;
                int lastFrame = _game.GetLatestFrame()?.Frame ?? _frames.Count;
                while (actionsLeft != 0 && framesLeft != 0 && currentFrame < lastFrame)
                {
                    futureComputer.ProcessFrame(_frames[currentFrame], _frames);

                    if (futureAction is not null)
                    {
                        if (pendingAction.CancelIf is not null && pendingAction.CancelIf(futureAction))
                        {
                            return; // cancelled
                        }

                        if (pendingAction.ContinuationIf is not null && pendingAction.ContinuationIf(futureAction))
                        {
                            ActionEvent continuationAction = pendingAction.Action with { HasContinuation = true };
                            InterpretActionEvent(continuationAction);

                            if (pendingAction.AppendContinuationWith is not null)
                            {
                                if (pendingAction.AppendContinuationWithIf is null || pendingAction.AppendContinuationWithIf(futureAction))
                                {
                                    InterpretActionEvent(pendingAction.AppendContinuationWith);
                                }
                            }

                            return;
                        }

                        if (actionsLeft != -1)
                        {
                            actionsLeft--;
                        }

                        futureAction = null;
                    }

                    if (framesLeft != -1)
                    {
                        framesLeft--;
                    }

                    currentFrame++;
                }

                // no early return so we're good to push it
                InterpretActionEvent(pendingAction.Action);

                futureComputer.OnAction -= OnFutureAction;
                futureComputer.OnRawAction -= OnFutureRawAction;
            }
        }
        else
        {
            int currentFrame = currentEvent.Frame;
            List<PendingAction> toKeep = [];
            List<PendingAction> toPush = [];
            foreach (var pendingEvent in _pendingBuffer.OrderBy(o => o.Action.Frame))
            {
                if (pendingEvent.CancelIf is not null && pendingEvent.CancelIf(currentEvent))
                {
                    if (LOG_VERBOSE)
                    {
                        Console.Write($" Cancel: {pendingEvent.Action.Action.ToString()} ({currentEvent.Action.ToString()})");
                    }
                    continue;
                }

                if (pendingEvent.ContinuationIf is not null && pendingEvent.ContinuationIf(currentEvent))
                {
                    toPush.Add(pendingEvent with
                    {
                        Action = pendingEvent.Action with { HasContinuation = true }
                    });

                    continue;
                }

                if (pendingEvent.FlushIf is not null && pendingEvent.FlushIf(currentEvent))
                {
                    toPush.Add(pendingEvent);

                    continue;
                }

                if (pendingEvent.FramesLeft != -1 && currentFrame - pendingEvent.Action.Frame >= pendingEvent.FramesLeft)
                {
                    toPush.Add(pendingEvent);

                    continue;
                }

                if (pendingEvent.ActionsLeft != -1 && pendingEvent.ActionsLeft == 1)
                {
                    toPush.Add(pendingEvent);

                    continue;
                }

                toKeep.Add(pendingEvent with
                {
                    FramesLeft = pendingEvent.FramesLeft == -1 ? -1 : pendingEvent.FramesLeft - (currentFrame - pendingEvent.Action.Frame),
                    ActionsLeft = pendingEvent.ActionsLeft == -1 ? -1 : pendingEvent.ActionsLeft - 1,
                });
            }

            _pendingBuffer = toKeep;

            foreach (var pendingEvent in toPush)
            {
                InterpretActionEvent(pendingEvent.Action);
                if (pendingEvent.Action.HasContinuation && pendingEvent.AppendContinuationWith is not null &&
                    (pendingEvent.AppendContinuationWithIf is null || pendingEvent.AppendContinuationWithIf(currentEvent)))
                {
                    InterpretActionEvent(pendingEvent.AppendContinuationWith);
                }
            }
        }
    }

    protected virtual Actions ComputeActionFromActionState(ActionState actionState)
    {
        Actions overrideAction = Actions.None;
        switch (actionState)
        {
            case ActionState.DASH:
                overrideAction = Actions.Dash;
                break;
            case ActionState.JUMP_BACKWARD:
            case ActionState.JUMP_FORWARD:
            case ActionState.JUMP_AERIAL_FORWARD:
            case ActionState.JUMP_AERIAL_BACKWARD:
            case ActionState.CLIFF_JUMP_QUICK_1:
            case ActionState.CLIFF_JUMP_QUICK_2:
            case ActionState.CLIFF_JUMP_SLOW_1:
            case ActionState.CLIFF_JUMP_SLOW_2:
                overrideAction = Actions.Jump;
                break;
            case ActionState.GROUNDED_CONTROL_END:
                overrideAction = Actions.JumpCancel;
                break;
            case ActionState.WALLJUMP:
                overrideAction = Actions.WallJump;
                break;
            case ActionState.GUARD:
                overrideAction = Actions.Shield;
                break;
            case ActionState.GUARD_START:
                overrideAction = Actions.ShieldStart;
                break;
            case ActionState.PASS:
                overrideAction = Actions.PlatformDrop;
                break;
            default:
                break;
        }

        return overrideAction;
    }

    protected virtual void InterpretActionEvent(ActionEvent actionEvent)
    {
        SimpleButtons buttons = actionEvent.FrameEntry.Players![_playerIndex]!.Pre!.ToSimpleButtons();
        bool facingLeft = (actionEvent.FrameEntry.Players![_playerIndex]!.Post!.FacingDirection ?? 0) < 0;

        switch (actionEvent.Action)
        {
            case Actions.Jab:
                {
                    _combos.Add(new InterpretedCombo()
                    {
                        ActionEvent = actionEvent,
                        DisplayName = "jab",
                        HasContinuation = actionEvent.HasContinuation,
                        Buttons = SimpleButtons.A,
                        EndsCombo = false,
                    });

                    break;
                }
            case Actions.Bair:
                {
                    _combos.Add(new InterpretedCombo()
                    {
                        ActionEvent = actionEvent,
                        DisplayName = "bair",
                        HasContinuation = false,
                        Buttons = Utils.FacingDirectionToOppositeCstick(facingLeft),
                        EndsCombo = true,
                    });

                    break;
                }
            case Actions.DAir:
                {
                    _combos.Add(new InterpretedCombo()
                    {
                        ActionEvent = actionEvent,
                        DisplayName = "drill",
                        HasContinuation = actionEvent.HasContinuation,
                        Buttons = SimpleButtons.CSTICK_DOWN,
                        EndsCombo = false,
                    });

                    break;
                }
            case Actions.Fair:
                {
                    _combos.Add(new InterpretedCombo()
                    {
                        ActionEvent = actionEvent,
                        DisplayName = "fair",
                        HasContinuation = actionEvent.HasContinuation,
                        Buttons = Utils.FacingDirectionToCstick(facingLeft),
                        EndsCombo = actionEvent.HasContinuation,
                    });

                    break;
                }
            case Actions.UAir:
                {
                    _combos.Add(new InterpretedCombo()
                    {
                        ActionEvent = actionEvent,
                        DisplayName = "upair",
                        HasContinuation = actionEvent.HasContinuation,
                        Buttons = SimpleButtons.CSTICK_UP,
                        EndsCombo = actionEvent.HasContinuation,
                    });

                    break;
                }
            case Actions.Nair:
                {
                    _combos.Add(new InterpretedCombo()
                    {
                        ActionEvent = actionEvent,
                        DisplayName = "nair",
                        HasContinuation = actionEvent.HasContinuation,
                        Buttons = SimpleButtons.A,
                        EndsCombo = actionEvent.HasContinuation,
                    });

                    break;
                }
            case Actions.USmash:
                {
                    _combos.Add(new InterpretedCombo()
                    {
                        ActionEvent = actionEvent,
                        DisplayName = "upsmash",
                        HasContinuation = false,
                        Buttons = SimpleButtons.CSTICK_UP,
                        EndsCombo = true,
                    });

                    break;
                }
            case Actions.FSmash:
                {
                    _combos.Add(new InterpretedCombo()
                    {
                        ActionEvent = actionEvent,
                        DisplayName = "fsmash",
                        HasContinuation = false,
                        Buttons = Utils.FacingDirectionToCstick(facingLeft),
                        EndsCombo = true,
                    });

                    break;
                }
            case Actions.DSmash:
                {
                    _combos.Add(new InterpretedCombo()
                    {
                        ActionEvent = actionEvent,
                        DisplayName = "dsmash",
                        HasContinuation = false,
                        Buttons = SimpleButtons.CSTICK_DOWN,
                        EndsCombo = true,
                    });

                    break;
                }
            case Actions.UTilt:
                {
                    _combos.Add(new InterpretedCombo()
                    {
                        ActionEvent = actionEvent,
                        DisplayName = "uptilt",
                        HasContinuation = false,
                        Buttons = SimpleButtons.A | SimpleButtons.STICK_UP,
                        EndsCombo = false,
                    });

                    break;
                }
            case Actions.FTilt:
                {
                    _combos.Add(new InterpretedCombo()
                    {
                        ActionEvent = actionEvent,
                        DisplayName = "ftilt",
                        HasContinuation = false,
                        Buttons = Utils.FacingDirectionToStick(facingLeft) | SimpleButtons.A,
                        EndsCombo = false,
                    });

                    break;
                }
            case Actions.DTilt:
                {
                    _combos.Add(new InterpretedCombo()
                    {
                        ActionEvent = actionEvent,
                        DisplayName = "dtilt",
                        HasContinuation = false,
                        Buttons = SimpleButtons.STICK_DOWN | SimpleButtons.A,
                        EndsCombo = false,
                    });

                    break;
                }
            case Actions.Shine:
                {
                    _combos.Add(new InterpretedCombo()
                    {
                        ActionEvent = actionEvent,
                        DisplayName = "shine",
                        HasContinuation = actionEvent.HasContinuation,
                        Buttons = SimpleButtons.STICK_DOWN | SimpleButtons.B,
                        EndsCombo = false,
                    });

                    break;
                }
            case Actions.FoxSideB:
                {
                    _combos.Add(new InterpretedCombo()
                    {
                        ActionEvent = actionEvent,
                        DisplayName = "side b",
                        HasContinuation = false,
                        Buttons = Utils.FacingDirectionToStick(facingLeft) | SimpleButtons.B,
                        EndsCombo = true, // TODO teeter cancel
                    });

                    break;
                }
            case Actions.FirefoxStartup:
                {
                    _combos.Add(new InterpretedCombo()
                    {
                        ActionEvent = actionEvent,
                        DisplayName = "up b",
                        HasContinuation = false,
                        Buttons = SimpleButtons.STICK_UP | SimpleButtons.B,
                        EndsCombo = false,
                    });

                    break;
                }
            case Actions.Laser:
                {
                    _combos.Add(new InterpretedCombo()
                    {
                        ActionEvent = actionEvent,
                        DisplayName = "laser",
                        HasContinuation = false,
                        Buttons = SimpleButtons.B,
                        EndsCombo = true,
                    });

                    break;
                }
            case Actions.DashAttack:
                {
                    _combos.Add(new InterpretedCombo()
                    {
                        ActionEvent = actionEvent,
                        DisplayName = "dash attack",
                        HasContinuation = actionEvent.HasContinuation,
                        Buttons = SimpleButtons.A,
                        EndsCombo = false,
                    });

                    break;
                }
            case Actions.Grab:
                {
                    _combos.Add(new InterpretedCombo()
                    {
                        ActionEvent = actionEvent,
                        DisplayName = "grab",
                        HasContinuation = false,
                        Buttons = SimpleButtons.Z,
                        EndsCombo = false,
                    });

                    break;
                }
            case Actions.UThrow:
                {
                    _combos.Add(new InterpretedCombo()
                    {
                        ActionEvent = actionEvent,
                        DisplayName = "upthrow",
                        HasContinuation = false,
                        Buttons = SimpleButtons.STICK_UP,
                        EndsCombo = false,
                    });

                    break;
                }
            case Actions.FThrow:
                {
                    _combos.Add(new InterpretedCombo()
                    {
                        ActionEvent = actionEvent,
                        DisplayName = $"{(facingLeft ? "f" : "➜")}throw",
                        HasContinuation = false,
                        Buttons = Utils.FacingDirectionToStick(facingLeft),
                        EndsCombo = false,
                    });

                    break;
                }
            case Actions.BThrow:
                {
                    _combos.Add(new InterpretedCombo()
                    {
                        ActionEvent = actionEvent,
                        DisplayName = $"{(!facingLeft ? "back" : "➜")}throw",
                        HasContinuation = false,
                        Buttons = Utils.FacingDirectionToOppositeStick(facingLeft),
                        EndsCombo = false,
                    });

                    break;
                }
            case Actions.DThrow:
                {
                    _combos.Add(new InterpretedCombo()
                    {
                        ActionEvent = actionEvent,
                        DisplayName = "downthrow",
                        HasContinuation = false,
                        Buttons = SimpleButtons.STICK_DOWN,
                        EndsCombo = false,
                    });

                    break;
                }
            case Actions.LCancel:
                {
                    _combos.Add(new InterpretedCombo()
                    {
                        ActionEvent = actionEvent,
                        DisplayName = "lcancel",
                        HasContinuation = false,
                        Buttons = Utils.GetTriggerButton(buttons),
                        EndsCombo = false,
                    });

                    break;
                }
            case Actions.DashDance:
                {
                    SimpleButtons direction = buttons.HasFlag(SimpleButtons.STICK_LEFT) ? SimpleButtons.STICK_LEFT : SimpleButtons.STICK_RIGHT;
                    _combos.Add(new InterpretedCombo()
                    {
                        ActionEvent = actionEvent,
                        DisplayName = $"dd",
                        HasContinuation = false,
                        Buttons = direction,
                        EndsCombo = false,
                    });

                    break;
                }
            case Actions.SpotDodge:
                {
                    _combos.Add(new InterpretedCombo()
                    {
                        ActionEvent = actionEvent,
                        DisplayName = "spotdodge",
                        HasContinuation = false,
                        Buttons = buttons, // TODO
                        EndsCombo = false,
                    });

                    break;
                }
            case Actions.Roll:
                {
                    _combos.Add(new InterpretedCombo()
                    {
                        ActionEvent = actionEvent,
                        DisplayName = "roll",
                        HasContinuation = false,
                        Buttons = Utils.GetTriggerButton(buttons) | Utils.FacingDirectionToStick(facingLeft),
                        EndsCombo = false,
                    });

                    break;
                }
            case Actions.Tech:
                {
                    _combos.Add(new InterpretedCombo()
                    {
                        ActionEvent = actionEvent,
                        DisplayName = "tech",
                        HasContinuation = false,
                        Buttons = Utils.GetTriggerButton(buttons),
                        EndsCombo = false,
                    });

                    break;
                }
            case Actions.AirDodge:
                {
                    _combos.Add(new InterpretedCombo()
                    {
                        ActionEvent = actionEvent,
                        DisplayName = $"air dodge",
                        HasContinuation = false,
                        Buttons = Utils.GetTriggerButton(buttons) | Utils.GetStick(buttons),
                        EndsCombo = false,
                    });

                    break;
                }
            case Actions.Wavedash:
                {
                    SimpleButtons stickDirection = buttons.HasFlag(SimpleButtons.STICK_LEFT) ? SimpleButtons.STICK_LEFT : SimpleButtons.STICK_RIGHT;
                    _combos.Add(new InterpretedCombo()
                    {
                        ActionEvent = actionEvent,
                        DisplayName = $"wavedash",
                        HasContinuation = actionEvent.HasContinuation,
                        Buttons = stickDirection | SimpleButtons.STICK_DOWN,
                        EndsCombo = false,
                    });

                    break;
                }
            case Actions.Waveland:
                {
                    SimpleButtons stickDirection = buttons.HasFlag(SimpleButtons.STICK_LEFT) ? SimpleButtons.STICK_LEFT : SimpleButtons.STICK_RIGHT;
                    _combos.Add(new InterpretedCombo()
                    {
                        ActionEvent = actionEvent,
                        DisplayName = $"waveland",
                        HasContinuation = actionEvent.HasContinuation,
                        Buttons = stickDirection | SimpleButtons.STICK_DOWN,
                        EndsCombo = false,
                    });

                    break;
                }
            case Actions.Dash:
                {
                    SimpleButtons direction = Utils.FacingDirectionToStick(facingLeft);
                    _combos.Add(new InterpretedCombo()
                    {
                        ActionEvent = actionEvent,
                        DisplayName = $"dash",
                        HasContinuation = false,
                        Buttons = direction,
                        EndsCombo = false,
                    });

                    break;
                }
            case Actions.Jump:
                {
                    SimpleButtons jumpButton = Utils.GetJumpButton(buttons);
                    _combos.Add(new InterpretedCombo()
                    {
                        ActionEvent = actionEvent,
                        DisplayName = "jump",
                        HasContinuation = false,
                        Buttons = jumpButton,
                        EndsCombo = false,
                    });

                    break;
                }
            case Actions.JumpCancel:
                {
                    _eventBuffer.Add(actionEvent); // HACK 

                    SimpleButtons jumpButton = Utils.GetJumpButton(buttons);
                    _combos.Add(new InterpretedCombo()
                    {
                        ActionEvent = actionEvent,
                        DisplayName = "jc",
                        HasContinuation = actionEvent.HasContinuation,
                        Buttons = jumpButton,
                        EndsCombo = false,
                    });

                    break;
                }
            case Actions.ShineTurnaround:
                {
                    SimpleButtons direction = Utils.FacingDirectionToStick(facingLeft);
                    _combos.Add(new InterpretedCombo()
                    {
                        ActionEvent = actionEvent,
                        DisplayName = direction == SimpleButtons.STICK_LEFT ? "↩" : "↪",
                        HasContinuation = actionEvent.HasContinuation,
                        Buttons = direction,
                        EndsCombo = false,
                    });

                    break;
                }
            case Actions.Firefox:
                {
                    SimpleButtons direction = Utils.GetStick(buttons);
                    _combos.Add(new InterpretedCombo()
                    {
                        ActionEvent = actionEvent,
                        DisplayName = "up b",
                        HasContinuation = false,
                        Buttons = direction | SimpleButtons.B,
                        EndsCombo = true,
                    });

                    break;
                }
            case Actions.ShieldDrop:
                {
                    SimpleButtons stick = Utils.GetStick(buttons);
                    _combos.Add(new InterpretedCombo()
                    {
                        ActionEvent = actionEvent,
                        DisplayName = "shield drop",
                        HasContinuation = actionEvent.HasContinuation,
                        Buttons = stick | Utils.GetTriggerButton(buttons),
                        EndsCombo = false,
                    });

                    break;
                }
            case Actions.PlatformDrop:
                {
                    _combos.Add(new InterpretedCombo()
                    {
                        ActionEvent = actionEvent,
                        DisplayName = "drop",
                        HasContinuation = false,
                        Buttons = SimpleButtons.STICK_DOWN,
                        EndsCombo = false,
                    });

                    break;
                }
            case Actions.WallJump:
                {
                    _combos.Add(new InterpretedCombo()
                    {
                        ActionEvent = actionEvent,
                        DisplayName = "walljump",
                        HasContinuation = actionEvent.HasContinuation,
                        Buttons = Utils.FacingDirectionToStick(facingLeft),
                        EndsCombo = false
                    });

                    break;
                }
            case Actions.FastFall:
                {
                    _combos.Add(new InterpretedCombo()
                    {
                        ActionEvent = actionEvent,
                        DisplayName = "ff",
                        HasContinuation = false,
                        Buttons = SimpleButtons.STICK_DOWN,
                        EndsCombo = false,
                    });

                    break;
                }
            default:
                break;
        }
    }

    // assuming the platform drop is already in the buffer
    protected bool IsFallThroughShieldDrop() => _eventBuffer.Count > 3 &&
        (_eventBuffer[^2].Action == Actions.ShieldStart ||
        (_eventBuffer[^2].Action == Actions.Shield && _eventBuffer[^3].Action == Actions.ShieldStart));

    public virtual void Dispose()
    {
        _actionsComputer.OnAction -= OnAction;

        if (_game is not null)
        {
            _game.OnGameEnd -= OnGameEnd;
            _game.Dispose();
        }
    }
}
