using Slippi.NET.Stats.Types;
using Slippi.NET.Types;

namespace Slippi.NET.Stats.Utils;

public static class StatsUtils
{
    public static IList<PlayerIndexedType> GetSinglesPlayerPermutationsFromSettings(GameStart settings)
    {
        if (settings?.Players.Count != 2)
        {
            return [];
        }

        return 
        [
            new PlayerIndexedType
            {
                PlayerIndex = settings.Players[0].PlayerIndex,
                OpponentIndex = settings.Players[1].PlayerIndex
            },
            new PlayerIndexedType
            {
                PlayerIndex = settings.Players[1].PlayerIndex,
                OpponentIndex = settings.Players[0].PlayerIndex
            }
        ];
    }

    public static bool DidLoseStock(PostFrameUpdate frame, PostFrameUpdate prevFrame)
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

    public static bool IsInControl(State state)
    {
        bool ground = state >= State.GROUNDED_CONTROL_START && state <= State.GROUNDED_CONTROL_END;
        bool squat = state >= State.SQUAT_START && state <= State.SQUAT_END;
        bool groundAttack = state > State.GROUND_ATTACK_START && state <= State.GROUND_ATTACK_END;
        bool isGrab = state == State.GRAB;

        return ground || squat || groundAttack || isGrab;
    }

    public static bool IsTeching(State state)
    {
        return state >= State.TECH_START && state <= State.TECH_END;
    }

    public static bool IsDown(State state)
    {
        return state >= State.DOWN_START && state <= State.DOWN_END;
    }

    public static bool IsDamaged(State state)
    {
        return (state >= State.DAMAGE_START && state <= State.DAMAGE_END) ||
               state == State.DAMAGE_FALL ||
               state == State.JAB_RESET_UP ||
               state == State.JAB_RESET_DOWN;
    }

    public static bool IsGrabbed(State state)
    {
        return state >= State.CAPTURE_START && state <= State.CAPTURE_END;
    }

    public static bool IsCommandGrabbed(State state)
    {
        return ((state >= State.COMMAND_GRAB_RANGE1_START && state <= State.COMMAND_GRAB_RANGE1_END) ||
                (state >= State.COMMAND_GRAB_RANGE2_START && state <= State.COMMAND_GRAB_RANGE2_END)) &&
               state != State.BARREL_WAIT;
    }

    public static bool IsDead(State state)
    {
        return state >= State.DYING_START && state <= State.DYING_END;
    }
}