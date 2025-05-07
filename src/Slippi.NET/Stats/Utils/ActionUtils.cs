using Slippi.NET.Stats.Types;

namespace Slippi.NET.Stats.Utils;

public static class ActionUtils
{
    public static bool IsMissGroundTech(State animation)
    {
        return animation == State.TECH_MISS_DOWN || animation == State.TECH_MISS_UP;
    }

    public static bool IsRolling(State animation)
    {
        return animation == State.ROLL_BACKWARD || animation == State.ROLL_FORWARD;
    }

    public static bool IsGrabAction(State animation)
    {
        // Includes Grab pull, wait, pummel, and throws
        return animation > State.GRAB && animation <= State.THROW_DOWN && animation != State.DASH_GRAB;
    }

    public static bool IsGrabbing(State animation)
    {
        return animation == State.GRAB || animation == State.DASH_GRAB;
    }

    public static bool IsAerialAttack(State animation)
    {
        return animation >= State.AERIAL_ATTACK_START && animation <= State.AERIAL_ATTACK_END;
    }

    public static bool IsForwardTilt(State animation)
    {
        return animation >= State.ATTACK_FTILT_START && animation <= State.ATTACK_FTILT_END;
    }

    public static bool IsForwardSmash(State animation)
    {
        return animation >= State.ATTACK_FSMASH_START && animation <= State.ATTACK_FSMASH_END;
    }

    public static bool IsWavedashInitiationAnimation(State animation)
    {
        if (animation == State.AIR_DODGE)
        {
            return true;
        }

        var isAboveMin = animation >= State.CONTROLLED_JUMP_START;
        var isBelowMax = animation <= State.CONTROLLED_JUMP_END;
        return isAboveMin && isBelowMax;
    }
}