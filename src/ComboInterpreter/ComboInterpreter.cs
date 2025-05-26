using Slippi.NET;
using Slippi.NET.Melee.Data;
using Slippi.NET.Melee.Types;
using Slippi.NET.Slp.Parser;
using Slippi.NET.Slp.Parser.Types;
using Slippi.NET.Stats;
using Slippi.NET.Stats.Types;
using Slippi.NET.Types;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection.Metadata;

namespace ComboInterpreter;

public abstract class BaseComboInterpreter : IDisposable
{
    protected const bool LOG_VERBOSE = false;

    protected readonly bool _isReplay;
    protected readonly SlippiGame _game;

    protected readonly StatsComputer? _statsComputer;        // replay
    protected readonly Dictionary<int, FrameEntry>? _frames; // replay

    protected readonly ActionsComputer _actionsComputer;
    protected readonly int _playerIndex;

    protected readonly TaskCompletionSource _gameEnd;

    protected List<ActionEvent> _eventBuffer = [];
    protected List<PendingAction> _pendingBuffer = [];
    protected BlockingCollection<InterpretedCombo> _combos = new BlockingCollection<InterpretedCombo>();

    public BaseComboInterpreter(Character character, bool isReplay, int startFrame, string gamePath, params string[] netplayCodesOrNames)
    {
        _isReplay = isReplay;

        _actionsComputer = new ActionsComputer();
        _actionsComputer.OnAction += OnAction;
        _actionsComputer.OnRawAction += OnRawAction;

        _game = new SlippiGame(gamePath, new StatOptions() 
        { 
            ProcessOnTheFly = !isReplay, 
            FirstFrame = isReplay ? startFrame : (int)Frames.FIRST 
        }, customActionsComputer: isReplay ? null : _actionsComputer);
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
            _statsComputer.Register(_actionsComputer);
            _statsComputer.Setup(_game.GetSettings() ?? throw new Exception("Invalid replay"));

            _frames = _game.GetFrames();
        }

        int? candidatePlayerIndex = null;
        candidatePlayerIndex = _game.GetSettings()?.Players
                .Where(p => netplayCodesOrNames.Any(c => string.Equals(c, p.ConnectCode, StringComparison.Ordinal) ||
                                                         string.Equals(c, p.Nametag, StringComparison.Ordinal)))
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
                $"Searched for: {string.Join(",", [..netplayCodesOrNames])}\n" +
                $"Found: {string.Join(",", _game.GetMetadata()?.Players.Select(p => $"{p.Value.Names?.Netplay ?? "N/A"} / {p.Value.Names?.Code ?? "N/A"}") ?? [])}"
            );
        }
        else
        {
            _playerIndex = candidatePlayerIndex.Value;
            Console.WriteLine($"Player index: {_playerIndex}");
        }

        _game.GetStats();
    }

    public BlockingCollection<InterpretedCombo> ComboStream => _combos;



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

    protected Actions ComputeActionFromActionState(ActionState actionState)
    {
        Actions overrideAction = Actions.None;
        switch (actionState)
        {
            case ActionState.FOX_SHINE_A:
            case ActionState.FOX_SHINE_G:
                overrideAction = Actions.Shine;
                break;
            case ActionState.FOX_LASER_A:
            case ActionState.FOX_LASER_G:
                overrideAction = Actions.Laser;
                break;
            case ActionState.FOX_SHINE_TURNAROUND_A:
            case ActionState.FOX_SHINE_TURNAROUND_G:
                overrideAction = Actions.ShineTurnaround;
                break;
            case ActionState.FOX_SHINE_END_A:
            case ActionState.FOX_SHINE_END_G:
                overrideAction = Actions.ShineEnd;
                break;
            case ActionState.FOX_SIDEB_A:
            case ActionState.FOX_SIDEB_G:
                overrideAction = Actions.SideB;
                break;
            case ActionState.FOX_UPB_A_STARTUP:
            case ActionState.FOX_UPB_G_STARTUP:
                overrideAction = Actions.FirefoxStartup;
                break;
            case ActionState.FOX_UPB_A:
            case ActionState.FOX_UPB_G:
                overrideAction = Actions.Firefox;
                break;
            case ActionState.DASH:
                overrideAction = Actions.Dash;
                break;
            case ActionState.JUMP_BACKWARD:
            case ActionState.JUMP_FORWARD:
                overrideAction = Actions.Jump;
                break;
            case ActionState.GROUNDED_CONTROL_END:
                overrideAction = Actions.JumpCancel;
                break;
            default:
                break;
        }

        return overrideAction;
    }

    protected void InterpretActionEvent(ActionEvent actionEvent)
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
            case Actions.SideB:
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
                        DisplayName = "↑throw",
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
                        DisplayName = $"{(facingLeft ? "←" : "→")}throw",
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
                        DisplayName = $"{(!facingLeft ? "←" : "→")}throw",
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
                        DisplayName = "↓throw",
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
                        EndsCombo = true,
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
                        EndsCombo = true,
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
                        EndsCombo = true,
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
            default:
                break;
        }
    }

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
