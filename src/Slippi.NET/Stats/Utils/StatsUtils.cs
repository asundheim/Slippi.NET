using Slippi.NET.Stats.Types;
using Slippi.NET.Types;

namespace Slippi.NET.Stats.Utils;

public static class StatsUtils
{
    public static IList<PlayerIndices> GetSinglesPlayerPermutationsFromSettings(GameStart settings)
    {
        if (settings?.Players.Count != 2)
        {
            return [];
        }

        return 
        [
            new PlayerIndices
            {
                PlayerIndex = settings.Players[0].PlayerIndex,
                OpponentIndex = settings.Players[1].PlayerIndex
            },
            new PlayerIndices
            {
                PlayerIndex = settings.Players[1].PlayerIndex,
                OpponentIndex = settings.Players[0].PlayerIndex
            }
        ];
    }

    public static bool DidLoseStock(PostFrameUpdate? frame, PostFrameUpdate? prevFrame)
    {
        if (frame is null || prevFrame is null)
        {
            return false;
        }

        return prevFrame.StocksRemaining - frame.StocksRemaining > 0;
    }

    public static float CalcDamageTaken(PostFrameUpdate frame, PostFrameUpdate prevFrame)
    {
        var percent = frame.Percent ?? 0;
        var prevPercent = prevFrame.Percent ?? 0;

        return percent - prevPercent;
    }

    public static bool IsInControl(ActionState state)
    {
        bool ground = state >= ActionState.GROUNDED_CONTROL_START && state <= ActionState.GROUNDED_CONTROL_END;
        bool squat = state >= ActionState.SQUAT_START && state <= ActionState.SQUAT_END;
        bool groundAttack = state > ActionState.GROUND_ATTACK_START && state <= ActionState.GROUND_ATTACK_END;
        bool isGrab = state == ActionState.GRAB;

        return ground || squat || groundAttack || isGrab;
    }

    public static bool IsTeching(ActionState state)
    {
        return state >= ActionState.TECH_START && state <= ActionState.TECH_END;
    }

    public static bool IsDown(ActionState state)
    {
        return state >= ActionState.DOWN_START && state <= ActionState.DOWN_END;
    }

    public static bool IsDamaged(ActionState state)
    {
        return (state >= ActionState.DAMAGE_START && state <= ActionState.DAMAGE_END) ||
               state == ActionState.DAMAGE_FALL ||
               state == ActionState.JAB_RESET_UP ||
               state == ActionState.JAB_RESET_DOWN;
    }

    public static bool IsGrabbed(ActionState state)
    {
        return state >= ActionState.CAPTURE_START && state <= ActionState.CAPTURE_END;
    }

    public static bool IsCommandGrabbed(ActionState state)
    {
        return ((state >= ActionState.COMMAND_GRAB_RANGE1_START && state <= ActionState.COMMAND_GRAB_RANGE1_END) ||
                (state >= ActionState.COMMAND_GRAB_RANGE2_START && state <= ActionState.COMMAND_GRAB_RANGE2_END)) &&
               state != ActionState.BARREL_WAIT;
    }

    public static bool IsDead(ActionState state)
    {
        return state >= ActionState.DYING_START && state <= ActionState.DYING_END;
    }
}