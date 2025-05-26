using Slippi.NET.Melee.Types;
using Slippi.NET.Stats;
using Slippi.NET.Stats.Types;

namespace ComboInterpreter;

public class FoxComboInterpreter : BaseComboInterpreter
{
    public FoxComboInterpreter(string liveGamePath, params string[] netplayCodesOrNames) 
        : base(Character.Fox, false, -1, liveGamePath, netplayCodesOrNames)
    {
    }

    public FoxComboInterpreter(string replayPath, int startFrame, params string[] netplayCodesOrNames)
        : base(Character.Fox, true, startFrame, replayPath, netplayCodesOrNames)
    { 
    }

    protected override void OnAction(object? sender, ActionEventArgs args)
    {
        base.OnAction(sender, args);

        if (args.Action == Actions.None)
        {
            return;
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
                Console.WriteLine($"VERBOSE: {actionEvent.Action.ToString()}");
            }
            
            if (!_isReplay)
            {
                ProcessPendingActions(actionEvent);
            }

            switch (args.Action)
            {
                case Actions.Jab:
                    {
                        _pendingBuffer.Add(new PendingAction()
                        {
                            Action = actionEvent,
                            ContinuationIf = (static c => c.Action == Actions.USmash || c.Action == Actions.Bair || c.Action == Actions.UAir || c.Action == Actions.DSmash),
                            ActionsLeft = 3,
                            FramesLeft = 20,
                        });

                        break;
                    }
                case Actions.AirDodge:
                    {
                        _pendingBuffer.Add(new PendingAction()
                        {
                            Action = actionEvent,
                            CancelIf = (static c => c.Action == Actions.Wavedash || c.Action == Actions.Waveland),
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
                case Actions.Shine:
                    {
                        _pendingBuffer.Add(new PendingAction()
                        {
                            Action = actionEvent,
                            ActionsLeft = 5,
                            FlushIf = static (c) => c.Action == Actions.ShineEnd,
                            ContinuationIf = static (c) => c.Action == Actions.Bair ||
                                                           c.Action == Actions.Nair ||
                                                           c.Action == Actions.DAir ||
                                                           c.Action == Actions.Fair ||
                                                           c.Action == Actions.UAir ||
                                                           c.Action == Actions.USmash ||
                                                           c.Action == Actions.DSmash ||
                                                           c.Action == Actions.FSmash ||
                                                           c.Action == Actions.Jab ||
                                                           c.Action == Actions.Grab ||
                                                           c.Action == Actions.ShineTurnaround ||
                                                           c.Action == Actions.Jump ||
                                                           c.Action == Actions.Wavedash ||
                                                           c.Action == Actions.Waveland,
                            AppendContinuationWithIf = static (c) => c.Action != Actions.Jump && c.Action != Actions.Wavedash,
                            AppendContinuationWith = new ActionEvent()
                            {
                                Action = Actions.JumpCancel,
                                FrameEntry = args.Frame,
                                HasContinuation = true,
                            }
                        });

                        break;
                    }
                case Actions.ShineTurnaround:
                    {
                        _pendingBuffer.Add(new PendingAction()
                        {
                            Action = actionEvent,
                            ActionsLeft = 3,
                            ContinuationIf = static (c) => c.Action == Actions.Bair ||
                                                           c.Action == Actions.Nair ||
                                                           c.Action == Actions.DAir ||
                                                           c.Action == Actions.Fair ||
                                                           c.Action == Actions.UAir ||
                                                           c.Action == Actions.USmash ||
                                                           c.Action == Actions.DSmash ||
                                                           c.Action == Actions.FSmash ||
                                                           c.Action == Actions.Jab ||
                                                           c.Action == Actions.Grab ||
                                                           c.Action == Actions.ShineTurnaround ||
                                                           c.Action == Actions.Jump ||
                                                           c.Action == Actions.Wavedash ||
                                                           c.Action == Actions.Waveland
                        });

                        break;
                    }
                case Actions.JumpCancel:
                    {
                        _pendingBuffer.Add(new PendingAction()
                        {
                            Action = actionEvent,
                            ActionsLeft = 1,
                            ContinuationIf = static (c) => c.Action == Actions.USmash ||
                                                           c.Action == Actions.Grab ||
                                                           c.Action == Actions.Shine,
                            CancelIf = static (c) => c.Action != Actions.USmash &&
                                                     c.Action != Actions.Grab &&
                                                     c.Action != Actions.Shine,
                        });

                        break;
                    }
                case Actions.Jump:
                    {
                        if (DidShineRecently())
                        {
                            _pendingBuffer.Add(new PendingAction()
                            {
                                Action = new ActionEvent() { Action = Actions.JumpCancel, FrameEntry = args.Frame },
                                ActionsLeft = 1,
                                ContinuationIf = static (c) => c.Action == Actions.Bair ||
                                                               c.Action == Actions.Nair ||
                                                               c.Action == Actions.DAir ||
                                                               c.Action == Actions.Fair ||
                                                               c.Action == Actions.UAir ||
                                                               c.Action == Actions.USmash ||
                                                               c.Action == Actions.Grab ||
                                                               c.Action == Actions.Jab,
                                CancelIf = static (c) => c.Action == Actions.Wavedash ||
                                                         c.Action == Actions.Waveland ||
                                                         c.Action == Actions.AirDodge

                            });
                        }
                        else
                        {
                            _pendingBuffer.Add(new PendingAction()
                            {
                                Action = actionEvent,
                                FramesLeft = 8,
                                CancelIf = static (c) => c.Action == Actions.Wavedash ||
                                                         c.Action == Actions.Waveland ||
                                                         c.Action == Actions.AirDodge
                            });
                        }

                        break;
                    }
                case Actions.Grab:
                case Actions.Laser:
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
                case Actions.FirefoxStartup:
                case Actions.Firefox:
                case Actions.SideB:
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
                    InterpretActionEvent(actionEvent);
                    break;
                default:
                    if (args.Action != Actions.None)
                    {
                        if (LOG_VERBOSE)
                        {
                            Console.Write($" Skip: {args.Action.ToString()} ");
                        }
                    }
                    break;
            }

            if (_isReplay)
            {
                ProcessPendingActions(actionEvent);
            }
        }
    }

    private const bool log_all = false;
    private const bool log_fox = false;

    protected override void OnRawAction(object? sender, RawActionEventArgs args)
    {
        base.OnRawAction(sender, args);
        
        if (args.PlayerIndex == _playerIndex)
        {
            int frame = args.Frame.Frame!.Value;

            Actions overrideAction = ComputeActionFromActionState(args.ActionState);
            if (overrideAction != Actions.None)
            {
                OnAction(sender, new ActionEventArgs() { Action = overrideAction, Frame = args.Frame, PlayerIndex = args.PlayerIndex });
            }

            if (log_all)
            {
                Console.WriteLine(args.ActionState.ToString());
                return;
            }

            if (log_fox)
            {
                LogFoxActionStateInfo(args);
            }
        }
    }

    private void LogFoxActionStateInfo(RawActionEventArgs args)
    {
        string foxAction = string.Empty;
        switch (args.ActionState)
        {
            case ActionState.FOX_SHINE_A:
            case ActionState.FOX_SHINE_G:
                foxAction = "Shine";
                break;
            case ActionState.FOX_LASER_A:
            case ActionState.FOX_LASER_G:
                foxAction = "Laser";
                break;
            case ActionState.FOX_SHINE_TURNAROUND_A:
            case ActionState.FOX_SHINE_TURNAROUND_G:
                foxAction = "Turnaround (Shine)";
                break;
            case ActionState.FOX_SIDEB_A:
            case ActionState.FOX_SIDEB_G:
                foxAction = "Side B";
                break;
            case ActionState.FOX_UPB_A_STARTUP:
            case ActionState.FOX_UPB_G_STARTUP:
                foxAction = "Up B";
                break;
            case ActionState.DASH:
                foxAction = "Dash";
                break;
            case ActionState.JUMP_BACKWARD:
            case ActionState.JUMP_FORWARD:
                foxAction = "Jump";
                break;
            default:
                break;
        }

        if (foxAction != string.Empty)
        {
            Console.WriteLine(foxAction);
        }
    }

    private bool DidShineRecently() => _eventBuffer.Count > 3 && (
                                            _eventBuffer[^1].Action == Actions.Shine ||
                                            _eventBuffer[^2].Action == Actions.Shine ||
                                            _eventBuffer[^3].Action == Actions.Shine);
}
