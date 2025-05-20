using Slippi.NET.Stats.Types;

namespace Slippi.NET.Stats.Utils;

public static class ActionUtils
{
    public static bool IsMissGroundTech(ActionState animation)
    {
        return animation == ActionState.TECH_MISS_DOWN || animation == ActionState.TECH_MISS_UP;
    }

    public static bool IsRolling(ActionState animation)
    {
        return animation == ActionState.ROLL_BACKWARD || animation == ActionState.ROLL_FORWARD;
    }

    public static bool IsGrabAction(ActionState animation)
    {
        // Includes Grab pull, wait, pummel, and throws
        return animation > ActionState.GRAB && animation <= ActionState.THROW_DOWN && animation != ActionState.DASH_GRAB;
    }

    public static bool IsGrabbing(ActionState animation)
    {
        return animation == ActionState.GRAB || animation == ActionState.DASH_GRAB;
    }

    public static bool IsAerialAttack(ActionState animation)
    {
        return animation >= ActionState.AERIAL_ATTACK_START && animation <= ActionState.AERIAL_ATTACK_END;
    }

    public static bool IsForwardTilt(ActionState animation)
    {
        return animation >= ActionState.ATTACK_FTILT_START && animation <= ActionState.ATTACK_FTILT_END;
    }

    public static bool IsForwardSmash(ActionState animation)
    {
        return animation >= ActionState.ATTACK_FSMASH_START && animation <= ActionState.ATTACK_FSMASH_END;
    }

    public static bool IsWavedashInitiationAnimation(ActionState animation)
    {
        if (animation == ActionState.AIR_DODGE)
        {
            return true;
        }

        var isAboveMin = animation >= ActionState.CONTROLLED_JUMP_START;
        var isBelowMax = animation <= ActionState.CONTROLLED_JUMP_END;
        return isAboveMin && isBelowMax;
    }
}