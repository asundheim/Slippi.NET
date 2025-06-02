using Slippi.NET.Stats.Types;
using Slippi.NET.Stats.Utils;
using Slippi.NET.Types;
using static Slippi.NET.Stats.Utils.ActionUtils;

namespace Slippi.NET.Stats;

public record class ActionEventArgs
{
    public required FrameEntry Frame { get; set; }
    public required Actions Action { get; set; }
    public required int PlayerIndex { get; set; }
}

public record class RawActionEventArgs
{
    public required FrameEntry Frame { get; set; }
    public required ActionState ActionState { get; set; }
    public required int PlayerIndex { get; set; }
}

public class ActionsComputer : IStatComputer<IList<ActionCounts>>
{
    private readonly Dictionary<PlayerIndices, PlayerActionState> _state = [];
    private List<PlayerIndices> _playerPermutations = [];

    public event EventHandler<ActionEventArgs>? OnAction;
    public event EventHandler<RawActionEventArgs>? OnRawAction;
    
    public void Setup(GameStart settings)
    {
        _state.Clear();
        _playerPermutations = StatsUtils.GetSinglesPlayerPermutationsFromSettings(settings).ToList();

        foreach (var indices in _playerPermutations)
        {
            var playerCounts = new ActionCounts
            {
                PlayerIndex = indices.PlayerIndex,
                WavedashCount = 0,
                WavelandCount = 0,
                AirDodgeCount = 0,
                DashDanceCount = 0,
                SpotDodgeCount = 0,
                LedgegrabCount = 0,
                RollCount = 0,
                LCancelCount = new LCancelCounts { Success = 0, Fail = 0 },
                AttackCount = new AttackCounts
                {
                    Jab1 = 0,
                    Jab2 = 0,
                    Jab3 = 0,
                    Jabm = 0,
                    Dash = 0,
                    Ftilt = 0,
                    Utilt = 0,
                    Dtilt = 0,
                    Fsmash = 0,
                    Usmash = 0,
                    Dsmash = 0,
                    Nair = 0,
                    Fair = 0,
                    Bair = 0,
                    Uair = 0,
                    Dair = 0
                },
                GrabCount = new GrabCounts { Success = 0, Fail = 0 },
                ThrowCount = new ThrowCounts { Up = 0, Forward = 0, Back = 0, Down = 0 },
                GroundTechCount = new GroundTechCounts { Away = 0, In = 0, Neutral = 0, Fail = 0 },
                WallTechCount = new WallTechCounts { Success = 0, Fail = 0 }
            };

            var playerState = new PlayerActionState
            {
                PlayerCounts = playerCounts,
                ActionStates = new List<ActionState>(),
                ActionFrameCounters = new List<float>()
            };

            _state[indices] = playerState;
        }
    }

    public void ProcessFrame(FrameEntry frame, Dictionary<int, FrameEntry> allFrames)
    {
        foreach (var indices in _playerPermutations)
        {
            if (_state.TryGetValue(indices, out PlayerActionState? state))
            {
                HandleActionCompute(state, indices, frame, allFrames);
            }
        }
    }

    public IList<ActionCounts> Fetch()
    {
        return _state.Values.Select(state => state.PlayerCounts).ToList();
    }

    private void HandleActionCompute(PlayerActionState state, PlayerIndices indices, FrameEntry frame, Dictionary<int, FrameEntry> allFrames)
    {
        var playerFrame = frame.Players![indices.PlayerIndex]!.Post;
        var opponentFrame = frame.Players[indices.OpponentIndex]!.Post;

        void ExecuteIf(Action execute, bool condition, Actions action = Actions.None)
        {
            if (!condition)
            {
                return;
            }

            execute();

            if (action != Actions.None)
            {
                OnAction?.Invoke(this, new ActionEventArgs() { Action = action, Frame = frame, PlayerIndex = indices.PlayerIndex });
            }
        }

        // Manage animation state
        ActionState currentActionState = (ActionState)playerFrame!.ActionStateId!.Value;
        state.ActionStates.Add(currentActionState);
        var currentFrameCounter = playerFrame.ActionStateCounter ?? 0; // not present in 0.1.0
        state.ActionFrameCounters.Add(currentFrameCounter);

        // Grab last 3 frames
        var last3ActionStates = state.ActionStates.TakeLast(3).ToList();

        var currFFState = playerFrame.StateFlags2?.HasFlag(StateFlags2.IsFastFalling);
        if (currFFState == true)
        {
            bool wasFastFalling = false;
            for (int i = 1; i < 10; i++)
            {
                if (allFrames.TryGetValue(frame.Frame!.Value - i, out FrameEntry? previousFrame))
                {
                    var prevFFState = previousFrame.Players?[indices.PlayerIndex]?.Post?.StateFlags2?.HasFlag(StateFlags2.IsFastFalling);

                    if (prevFFState == true)
                    {
                        wasFastFalling = true;
                        break;
                    }
                }
            }

            ExecuteIf(() => { }, !wasFastFalling, Actions.FastFall);
        }

        var prevAnimation = last3ActionStates.ElementAtOrDefault(last3ActionStates.Count - 2);
        var prevFrameCounter = state.ActionFrameCounters.ElementAtOrDefault(state.ActionFrameCounters.Count - 2);

        // New action if new animation or frame counter goes back down (repeated action)
        var isNewAction = currentActionState != prevAnimation || prevFrameCounter > currentFrameCounter;
        if (!isNewAction) return;

        OnRawAction?.Invoke(this, new RawActionEventArgs() { ActionState = currentActionState, PlayerIndex = indices.PlayerIndex, Frame = frame });

        // Increment counts based on conditions
        var didDashDance = last3ActionStates.Count == 3 && last3ActionStates[0] == ActionState.DASH && 
                                                     last3ActionStates[1] == ActionState.TURN && 
                                                     last3ActionStates[2] == ActionState.DASH;
        ExecuteIf(() => state.PlayerCounts.DashDanceCount++, didDashDance, Actions.DashDance);

        ExecuteIf(() => state.PlayerCounts.RollCount++, currentActionState.IsRolling(), Actions.Roll);
        ExecuteIf(() => state.PlayerCounts.SpotDodgeCount++, currentActionState == ActionState.SPOT_DODGE, Actions.SpotDodge);
        ExecuteIf(() => state.PlayerCounts.AirDodgeCount++, currentActionState == ActionState.AIR_DODGE, Actions.AirDodge);
        ExecuteIf(() => state.PlayerCounts.LedgegrabCount++, currentActionState == ActionState.CLIFF_CATCH, Actions.Ledgegrab);

        // Grabs
        ExecuteIf(() => state.PlayerCounts.GrabCount.Success++, prevAnimation.IsGrabbing() && currentActionState.IsGrabAction(), Actions.Grab);
        ExecuteIf(() => state.PlayerCounts.GrabCount.Fail++, prevAnimation.IsGrabbing() && !currentActionState.IsGrabAction(), Actions.Grab);
        if (currentActionState == ActionState.DASH_GRAB && prevAnimation == ActionState.ATTACK_DASH)
        {
            state.PlayerCounts.AttackCount.Dash -= 1; // subtract from dash attack if boost grab
        }

        // Basic attacks
        ExecuteIf(() => state.PlayerCounts.AttackCount.Jab1++, currentActionState == ActionState.ATTACK_JAB1, Actions.Jab);
        ExecuteIf(() => state.PlayerCounts.AttackCount.Jab2++, currentActionState == ActionState.ATTACK_JAB2, Actions.Jab);
        ExecuteIf(() => state.PlayerCounts.AttackCount.Jab3++, currentActionState == ActionState.ATTACK_JAB3, Actions.Jab);
        ExecuteIf(() => state.PlayerCounts.AttackCount.Jabm++, currentActionState == ActionState.ATTACK_JABM, Actions.Jab);
        ExecuteIf(() => state.PlayerCounts.AttackCount.Dash++, currentActionState == ActionState.ATTACK_DASH, Actions.DashAttack);
        ExecuteIf(() => state.PlayerCounts.AttackCount.Ftilt++, currentActionState.IsForwardTilt(), Actions.FTilt);
        ExecuteIf(() => state.PlayerCounts.AttackCount.Utilt++, currentActionState == ActionState.ATTACK_UTILT, Actions.UTilt);
        ExecuteIf(() => state.PlayerCounts.AttackCount.Dtilt++, currentActionState == ActionState.ATTACK_DTILT, Actions.DTilt);
        ExecuteIf(() => state.PlayerCounts.AttackCount.Fsmash++, currentActionState.IsForwardSmash(), Actions.FSmash);
        ExecuteIf(() => state.PlayerCounts.AttackCount.Usmash++, currentActionState == ActionState.ATTACK_USMASH, Actions.USmash);
        ExecuteIf(() => state.PlayerCounts.AttackCount.Dsmash++, currentActionState == ActionState.ATTACK_DSMASH, Actions.DSmash);
        ExecuteIf(() => state.PlayerCounts.AttackCount.Nair++, currentActionState == ActionState.AERIAL_NAIR, Actions.Nair);
        ExecuteIf(() => state.PlayerCounts.AttackCount.Fair++, currentActionState == ActionState.AERIAL_FAIR, Actions.Fair);
        ExecuteIf(() => state.PlayerCounts.AttackCount.Bair++, currentActionState == ActionState.AERIAL_BAIR, Actions.Bair);
        ExecuteIf(() => state.PlayerCounts.AttackCount.Uair++, currentActionState == ActionState.AERIAL_UAIR, Actions.UAir);
        ExecuteIf(() => state.PlayerCounts.AttackCount.Dair++, currentActionState == ActionState.AERIAL_DAIR, Actions.DAir);

        // GnW is weird and has unique IDs for some moves
        if (playerFrame.InternalCharacterId == 0x18)
        {
            ExecuteIf(() => state.PlayerCounts.AttackCount.Jab1++, currentActionState == ActionState.GNW_JAB1, Actions.Jab);
            ExecuteIf(() => state.PlayerCounts.AttackCount.Jabm++, currentActionState == ActionState.GNW_JABM, Actions.Jab);
            ExecuteIf(() => state.PlayerCounts.AttackCount.Dtilt++, currentActionState == ActionState.GNW_DTILT, Actions.DTilt);
            ExecuteIf(() => state.PlayerCounts.AttackCount.Fsmash++, currentActionState == ActionState.GNW_FSMASH, Actions.FSmash);
            ExecuteIf(() => state.PlayerCounts.AttackCount.Nair++, currentActionState == ActionState.GNW_NAIR, Actions.Nair);
            ExecuteIf(() => state.PlayerCounts.AttackCount.Bair++, currentActionState == ActionState.GNW_BAIR, Actions.Bair);
            ExecuteIf(() => state.PlayerCounts.AttackCount.Uair++, currentActionState == ActionState.GNW_UAIR, Actions.UAir);
        }

        // Peach is also weird and has a unique ID for her fsmash
        if (playerFrame.InternalCharacterId == 0x09)
        {
            ExecuteIf(() => state.PlayerCounts.AttackCount.Fsmash++, currentActionState == ActionState.PEACH_FSMASH1, Actions.FSmash);
            ExecuteIf(() => state.PlayerCounts.AttackCount.Fsmash++, currentActionState == ActionState.PEACH_FSMASH2, Actions.FSmash);
            ExecuteIf(() => state.PlayerCounts.AttackCount.Fsmash++, currentActionState == ActionState.PEACH_FSMASH3, Actions.FSmash);
        }

        // Throws
        ExecuteIf(() => state.PlayerCounts.ThrowCount.Up++, currentActionState == ActionState.THROW_UP, Actions.UThrow);
        ExecuteIf(() => state.PlayerCounts.ThrowCount.Forward++, currentActionState == ActionState.THROW_FORWARD, Actions.FThrow);
        ExecuteIf(() => state.PlayerCounts.ThrowCount.Down++, currentActionState == ActionState.THROW_DOWN, Actions.DThrow);
        ExecuteIf(() => state.PlayerCounts.ThrowCount.Back++, currentActionState == ActionState.THROW_BACK, Actions.BThrow);

        // Techs
        var opponentDir = playerFrame.PositionX > opponentFrame!.PositionX ? -1 : 1;
        var facingOpponent = playerFrame.FacingDirection == opponentDir;

        ExecuteIf(() => state.PlayerCounts.GroundTechCount.Fail++, (currentActionState).IsMissGroundTech());
        ExecuteIf(() => state.PlayerCounts.GroundTechCount.In++, currentActionState == ActionState.FORWARD_TECH && facingOpponent, Actions.Tech);
        ExecuteIf(() => state.PlayerCounts.GroundTechCount.In++, currentActionState == ActionState.BACKWARD_TECH && !facingOpponent, Actions.Tech);
        ExecuteIf(() => state.PlayerCounts.GroundTechCount.Neutral++, currentActionState == ActionState.NEUTRAL_TECH, Actions.Tech);
        ExecuteIf(() => state.PlayerCounts.GroundTechCount.Away++, currentActionState == ActionState.BACKWARD_TECH && facingOpponent, Actions.Tech);
        ExecuteIf(() => state.PlayerCounts.GroundTechCount.Away++, currentActionState == ActionState.FORWARD_TECH && !facingOpponent, Actions.Tech);
        ExecuteIf(() => state.PlayerCounts.WallTechCount.Success++, currentActionState == ActionState.WALL_TECH, Actions.Tech);
        ExecuteIf(() => state.PlayerCounts.WallTechCount.Fail++, currentActionState == ActionState.MISSED_WALL_TECH, Actions.Tech);

        if (currentActionState.IsAerialAttack())
        {
            ExecuteIf(() => state.PlayerCounts.LCancelCount.Success++, playerFrame.LCancelStatus == 1, Actions.LCancel);
            ExecuteIf(() => state.PlayerCounts.LCancelCount.Fail++, playerFrame.LCancelStatus == 2);
        }

        // Handles wavedash detection (and waveland)
        HandleActionWavedash(state.PlayerCounts, state.ActionStates, frame);
    }

    public void HandleActionWavedash(ActionCounts counts, List<ActionState> animations, FrameEntry frame)
    {
        if (animations.Count < 2)
        {
            return;
        }

        var currentAnimation = animations[^1];
        var prevAnimation = animations[^2];

        var isSpecialLanding = currentAnimation == ActionState.LANDING_FALL_SPECIAL;
        var isAcceptablePrevious = prevAnimation.IsWavedashInitiationAnimation();
        var isPossibleWavedash = isSpecialLanding && isAcceptablePrevious;

        if (!isPossibleWavedash)
        {
            return;
        }

        // Here we special landed, it might be a wavedash, let's check
        // We grab the last 8 frames here because that should be enough time to execute a
        // wavedash. This number could be tweaked if we find false negatives
        var recentFrames = animations.GetRange(Math.Max(0, animations.Count - 8), Math.Min(8, animations.Count));
        var recentAnimations = new HashSet<ActionState>(recentFrames);

        if (recentAnimations.Count == 2 && recentAnimations.Contains(ActionState.AIR_DODGE))
        {
            // If the only other animation is air dodge, this might be really late to the point
            // where it was actually an air dodge. Air dodge animation is really long
            return;
        }

        if (recentAnimations.Contains(ActionState.AIR_DODGE))
        {
            // If one of the recent animations was an air dodge, let's remove that from the
            // air dodge counter, we don't want to count air dodges used to wavedash/land
            counts.AirDodgeCount -= 1;
        }

        if (recentAnimations.Contains(ActionState.ACTION_KNEE_BEND))
        {
            // If a jump was started recently, we will consider this a wavedash
            counts.WavedashCount += 1;
            OnAction?.Invoke(this, new ActionEventArgs() { Action = Actions.Wavedash, Frame = frame, PlayerIndex = counts.PlayerIndex });
        }
        else
        {
            // If there was no jump recently, this is a waveland
            counts.WavelandCount += 1;
            OnAction?.Invoke(this, new ActionEventArgs() { Action = Actions.Waveland, Frame = frame, PlayerIndex = counts.PlayerIndex });
        }
    }
}
