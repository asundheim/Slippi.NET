using Slippi.NET.Stats.Types;
using Slippi.NET.Stats.Utils;
using Slippi.NET.Types;
using static Slippi.NET.Stats.Utils.ActionUtils;

namespace Slippi.NET.Stats;

public record class ActionsComputer : IStatComputer<IList<ActionCountsType>>
{
    private readonly Dictionary<PlayerIndexedType, PlayerActionState> _state = [];
    private List<PlayerIndexedType> _playerPermutations = [];
    
    public void Setup(GameStart settings)
    {
        _state.Clear();
        _playerPermutations = StatsUtils.GetSinglesPlayerPermutationsFromSettings(settings).ToList();

        foreach (var indices in _playerPermutations)
        {
            var playerCounts = new ActionCountsType
            {
                PlayerIndex = indices.PlayerIndex,
                WavedashCount = 0,
                WavelandCount = 0,
                AirDodgeCount = 0,
                DashDanceCount = 0,
                SpotDodgeCount = 0,
                LedgegrabCount = 0,
                RollCount = 0,
                LCancelCount = new LCancelCountType { Success = 0, Fail = 0 },
                AttackCount = new AttackCountType
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
                GrabCount = new GrabCountType { Success = 0, Fail = 0 },
                ThrowCount = new ThrowCountType { Up = 0, Forward = 0, Back = 0, Down = 0 },
                GroundTechCount = new GroundTechCountType { Away = 0, In = 0, Neutral = 0, Fail = 0 },
                WallTechCount = new WallTechCountType { Success = 0, Fail = 0 }
            };

            var playerState = new PlayerActionState
            {
                PlayerCounts = playerCounts,
                Animations = new List<int>(),
                ActionFrameCounters = new List<int>()
            };

            _state[indices] = playerState;
        }
    }

    public void ProcessFrame(FrameEntry frame, FramesType allFrames)
    {
        foreach (var indices in _playerPermutations)
        {
            if (_state.TryGetValue(indices, out PlayerActionState? state))
            {
                HandleActionCompute(state, indices, frame);
            }
        }
    }

    public IList<ActionCountsType> Fetch()
    {
        return _state.Values.Select(state => state.PlayerCounts).ToList();
    }

    private static void HandleActionCompute(PlayerActionState state, PlayerIndexedType indices, FrameEntry frame)
    {
        var playerFrame = frame.Players[indices.PlayerIndex]!.Post;
        var opponentFrame = frame.Players[indices.OpponentIndex]!.Post;

        static void ExecuteIf(Action execute, bool condition)
        {
            if (!condition)
            {
                return;
            }

            execute();
        }

        // Manage animation state
        int currentAnimation = playerFrame.ActionStateId!.Value;
        state.Animations.Add(currentAnimation);
        var currentFrameCounter = playerFrame.ActionStateCounter!.Value;
        state.ActionFrameCounters.Add(currentFrameCounter);

        // Grab last 3 frames
        var last3Frames = state.Animations.TakeLast(3).ToList();
        var prevAnimation = last3Frames.ElementAtOrDefault(last3Frames.Count - 2);
        var prevFrameCounter = state.ActionFrameCounters.ElementAtOrDefault(state.ActionFrameCounters.Count - 2);

        // New action if new animation or frame counter goes back down (repeated action)
        var isNewAction = currentAnimation != prevAnimation || prevFrameCounter > currentFrameCounter;
        if (!isNewAction) return;

        // Increment counts based on conditions
        var didDashDance = last3Frames.Count == 3 && last3Frames[0] == (int)State.DASH && 
                                                     last3Frames[1] == (int)State.TURN && 
                                                     last3Frames[2] == (int)State.DASH;
        ExecuteIf(() => state.PlayerCounts.DashDanceCount++, didDashDance);

        ExecuteIf(() => state.PlayerCounts.RollCount++, IsRolling((State)currentAnimation));
        ExecuteIf(() => state.PlayerCounts.SpotDodgeCount++, currentAnimation == (int)State.SPOT_DODGE);
        ExecuteIf(() => state.PlayerCounts.AirDodgeCount++, currentAnimation == (int)State.AIR_DODGE);
        ExecuteIf(() => state.PlayerCounts.LedgegrabCount++, currentAnimation == (int)State.CLIFF_CATCH);

        // Grabs
        ExecuteIf(() => state.PlayerCounts.GrabCount.Success++, IsGrabbing((State)prevAnimation) && IsGrabAction((State)currentAnimation));
        ExecuteIf(() => state.PlayerCounts.GrabCount.Fail++, IsGrabbing((State)prevAnimation) && !IsGrabAction((State)currentAnimation));
        if (currentAnimation == (int)State.DASH_GRAB && prevAnimation == (int)State.ATTACK_DASH)
        {
            state.PlayerCounts.AttackCount.Dash -= 1; // subtract from dash attack if boost grab
        }

        // Basic attacks
        ExecuteIf(() => state.PlayerCounts.AttackCount.Jab1++, currentAnimation == (int)State.ATTACK_JAB1);
        ExecuteIf(() => state.PlayerCounts.AttackCount.Jab2++, currentAnimation == (int)State.ATTACK_JAB2);
        ExecuteIf(() => state.PlayerCounts.AttackCount.Jab3++, currentAnimation == (int)State.ATTACK_JAB3);
        ExecuteIf(() => state.PlayerCounts.AttackCount.Jabm++, currentAnimation == (int)State.ATTACK_JABM);
        ExecuteIf(() => state.PlayerCounts.AttackCount.Dash++, currentAnimation == (int)State.ATTACK_DASH);
        ExecuteIf(() => state.PlayerCounts.AttackCount.Ftilt++, IsForwardTilt((State)currentAnimation));
        ExecuteIf(() => state.PlayerCounts.AttackCount.Utilt++, currentAnimation == (int)State.ATTACK_UTILT);
        ExecuteIf(() => state.PlayerCounts.AttackCount.Dtilt++, currentAnimation == (int)State.ATTACK_DTILT);
        ExecuteIf(() => state.PlayerCounts.AttackCount.Fsmash++, IsForwardSmash((State)currentAnimation));
        ExecuteIf(() => state.PlayerCounts.AttackCount.Usmash++, currentAnimation == (int)State.ATTACK_USMASH);
        ExecuteIf(() => state.PlayerCounts.AttackCount.Dsmash++, currentAnimation == (int)State.ATTACK_DSMASH);
        ExecuteIf(() => state.PlayerCounts.AttackCount.Nair++, currentAnimation == (int)State.AERIAL_NAIR);
        ExecuteIf(() => state.PlayerCounts.AttackCount.Fair++, currentAnimation == (int)State.AERIAL_FAIR);
        ExecuteIf(() => state.PlayerCounts.AttackCount.Bair++, currentAnimation == (int)State.AERIAL_BAIR);
        ExecuteIf(() => state.PlayerCounts.AttackCount.Uair++, currentAnimation == (int)State.AERIAL_UAIR);
        ExecuteIf(() => state.PlayerCounts.AttackCount.Dair++, currentAnimation == (int)State.AERIAL_DAIR);

        // GnW is weird and has unique IDs for some moves
        if (playerFrame.InternalCharacterId == 0x18)
        {
            ExecuteIf(() => state.PlayerCounts.AttackCount.Jab1++, currentAnimation == (int)State.GNW_JAB1);
            ExecuteIf(() => state.PlayerCounts.AttackCount.Jabm++, currentAnimation == (int)State.GNW_JABM);
            ExecuteIf(() => state.PlayerCounts.AttackCount.Dtilt++, currentAnimation == (int)State.GNW_DTILT);
            ExecuteIf(() => state.PlayerCounts.AttackCount.Fsmash++, currentAnimation == (int)State.GNW_FSMASH);
            ExecuteIf(() => state.PlayerCounts.AttackCount.Nair++, currentAnimation == (int)State.GNW_NAIR);
            ExecuteIf(() => state.PlayerCounts.AttackCount.Bair++, currentAnimation == (int)State.GNW_BAIR);
            ExecuteIf(() => state.PlayerCounts.AttackCount.Uair++, currentAnimation == (int)State.GNW_UAIR);
        }

        // Peach is also weird and has a unique ID for her fsmash
        if (playerFrame.InternalCharacterId == 0x09)
        {
            ExecuteIf(() => state.PlayerCounts.AttackCount.Fsmash++, currentAnimation == (int)State.PEACH_FSMASH1);
            ExecuteIf(() => state.PlayerCounts.AttackCount.Fsmash++, currentAnimation == (int)State.PEACH_FSMASH2);
            ExecuteIf(() => state.PlayerCounts.AttackCount.Fsmash++, currentAnimation == (int)State.PEACH_FSMASH3);
        }

        // Throws
        ExecuteIf(() => state.PlayerCounts.ThrowCount.Up++, currentAnimation == (int)State.THROW_UP);
        ExecuteIf(() => state.PlayerCounts.ThrowCount.Forward++, currentAnimation == (int)State.THROW_FORWARD);
        ExecuteIf(() => state.PlayerCounts.ThrowCount.Down++, currentAnimation == (int)State.THROW_DOWN);
        ExecuteIf(() => state.PlayerCounts.ThrowCount.Back++, currentAnimation == (int)State.THROW_BACK);

        // Techs
        var opponentDir = playerFrame.PositionX > opponentFrame.PositionX ? -1 : 1;
        var facingOpponent = playerFrame.FacingDirection == opponentDir;

        ExecuteIf(() => state.PlayerCounts.GroundTechCount.Fail++, IsMissGroundTech((State)currentAnimation));
        ExecuteIf(() => state.PlayerCounts.GroundTechCount.In++, currentAnimation == (int)State.FORWARD_TECH && facingOpponent);
        ExecuteIf(() => state.PlayerCounts.GroundTechCount.In++, currentAnimation == (int)State.BACKWARD_TECH && !facingOpponent);
        ExecuteIf(() => state.PlayerCounts.GroundTechCount.Neutral++, currentAnimation == (int)State.NEUTRAL_TECH);
        ExecuteIf(() => state.PlayerCounts.GroundTechCount.Away++, currentAnimation == (int)State.BACKWARD_TECH && facingOpponent);
        ExecuteIf(() => state.PlayerCounts.GroundTechCount.Away++, currentAnimation == (int)State.FORWARD_TECH && !facingOpponent);
        ExecuteIf(() => state.PlayerCounts.WallTechCount.Success++, currentAnimation == (int)State.WALL_TECH);
        ExecuteIf(() => state.PlayerCounts.WallTechCount.Fail++, currentAnimation == (int)State.MISSED_WALL_TECH);

        if (IsAerialAttack((State)currentAnimation))
        {
            ExecuteIf(() => state.PlayerCounts.LCancelCount.Success++, playerFrame.LCancelStatus == 1);
            ExecuteIf(() => state.PlayerCounts.LCancelCount.Fail++, playerFrame.LCancelStatus == 2);
        }

        // Handles wavedash detection (and waveland)
        HandleActionWavedash(state.PlayerCounts, state.Animations);
    }

    public static void HandleActionWavedash(ActionCountsType counts, List<int> animations)
    {
        var currentAnimation = animations[^1];
        var prevAnimation = animations[^2];

        var isSpecialLanding = currentAnimation == (int)State.LANDING_FALL_SPECIAL;
        var isAcceptablePrevious = IsWavedashInitiationAnimation((State)prevAnimation);
        var isPossibleWavedash = isSpecialLanding && isAcceptablePrevious;

        if (!isPossibleWavedash)
        {
            return;
        }

        // Here we special landed, it might be a wavedash, let's check
        // We grab the last 8 frames here because that should be enough time to execute a
        // wavedash. This number could be tweaked if we find false negatives
        var recentFrames = animations.GetRange(Math.Max(0, animations.Count - 8), Math.Min(8, animations.Count));
        var recentAnimations = new HashSet<int>(recentFrames);

        if (recentAnimations.Count == 2 && recentAnimations.Contains((int)State.AIR_DODGE))
        {
            // If the only other animation is air dodge, this might be really late to the point
            // where it was actually an air dodge. Air dodge animation is really long
            return;
        }

        if (recentAnimations.Contains((int)State.AIR_DODGE))
        {
            // If one of the recent animations was an air dodge, let's remove that from the
            // air dodge counter, we don't want to count air dodges used to wavedash/land
            counts.AirDodgeCount -= 1;
        }

        if (recentAnimations.Contains((int)State.ACTION_KNEE_BEND))
        {
            // If a jump was started recently, we will consider this a wavedash
            counts.WavedashCount += 1;
        }
        else
        {
            // If there was no jump recently, this is a waveland
            counts.WavelandCount += 1;
        }
    }
}
