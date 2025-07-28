using Slippi.NET.Melee.Types;
using Slippi.NET.Stats;
using Slippi.NET.Stats.Types;
using Slippi.NET.Types;

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

    protected override void HandleActionEvent(ActionEvent actionEvent)
    {
        switch (actionEvent.Action)
        {
            case Actions.Jab:
                {
                    _pendingBuffer.Add(new PendingAction()
                    {
                        Action = actionEvent,
                        ContinuationIf = static c => c.Action == Actions.USmash || c.Action == Actions.Bair || c.Action == Actions.UAir || c.Action == Actions.DSmash,
                        ActionsLeft = 3,
                        FramesLeft = 20,
                    });

                    break;
                }
            case Actions.Shine:
                {
                    _pendingBuffer.Add(new PendingAction()
                    {
                        Action = actionEvent,
                        ActionsLeft = 5,
                        FramesLeft = 20,
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
                        AppendContinuationWithIf = static (c) => c.Action != Actions.Jump && 
                                                                 c.Action != Actions.Wavedash && 
                                                                 c.Action != Actions.USmash && 
                                                                 c.Action != Actions.Grab && 
                                                                 c.Action != Actions.ShineTurnaround,
                        AppendContinuationWith = new ActionEvent()
                        {
                            Action = Actions.JumpCancel,
                            FrameEntry = actionEvent.FrameEntry,
                            HasContinuation = true,
                        },
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
                        CancelIf = static (c) => (c.Action != Actions.USmash &&
                                                 c.Action != Actions.Grab &&
                                                 c.Action != Actions.Shine) || c.Action == Actions.JumpCancel,
                    });

                    break;
                }
            case Actions.Jump:
                {
                    if (DidShineRecentlyForJumpCancel())
                    {
                        _pendingBuffer.Add(new PendingAction()
                        {
                            Action = new ActionEvent() { Action = Actions.JumpCancel, FrameEntry = actionEvent.FrameEntry },
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
            case Actions.WallJump:
                {
                    _pendingBuffer.Add(new PendingAction()
                    {
                        Action = actionEvent,
                        ActionsLeft = 1,
                        ContinuationIf = static (c) => c.Action == Actions.Bair || c.Action == Actions.Shine
                    });

                    break;
                }
            case Actions.FirefoxStartup:
            case Actions.Firefox:
            case Actions.Laser:
            case Actions.FoxSideB:
                {
                    InterpretActionEvent(actionEvent);
                    break;
                }
            default:
                {
                    base.HandleActionEvent(actionEvent);
                    break;
                }
        }
    }

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
        }
    }

    protected override Actions ComputeActionFromActionState(ActionState actionState)
    {
        Actions overrideAction = Actions.None; // tracepoint here with {actionState} to log all action states

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
                overrideAction = Actions.FoxSideB;
                break;
            case ActionState.FOX_UPB_A_STARTUP:
            case ActionState.FOX_UPB_G_STARTUP:
                overrideAction = Actions.FirefoxStartup;
                break;
            case ActionState.FOX_UPB_A:
            case ActionState.FOX_UPB_G:
                overrideAction = Actions.Firefox;
                break;
            default:
                break;
        }

        if (overrideAction != Actions.None)
        {
            return overrideAction;
        }

        return base.ComputeActionFromActionState(actionState);
    }

    private bool DidShineRecentlyForJumpCancel() => _eventBuffer.Count > 4 && 
                                      (_eventBuffer[^1].Action != Actions.JumpCancel) &&
                                      (_eventBuffer[^2].Action != Actions.JumpCancel) &&
                                      (_eventBuffer[^1].Action == Actions.Shine ||
                                       _eventBuffer[^2].Action == Actions.Shine ||
                                       _eventBuffer[^3].Action == Actions.Shine ||
                                       _eventBuffer[^4].Action == Actions.Shine);
}
