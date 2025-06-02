using Slippi.NET.Stats.Types;
using Slippi.NET.Types;

namespace Slippi.NET.Stats.Utils;

public static class ActionUtils
{
    public static bool IsMissGroundTech(this ActionState actionState)
    {
        return actionState == ActionState.TECH_MISS_DOWN || actionState == ActionState.TECH_MISS_UP;
    }

    public static bool IsRolling(this ActionState actionState)
    {
        return actionState == ActionState.ROLL_BACKWARD || actionState == ActionState.ROLL_FORWARD;
    }
     
    public static bool IsGrabAction(this ActionState actionState)
    {
        // Includes Grab pull, wait, pummel, and throws
        return actionState > ActionState.GRAB && actionState <= ActionState.THROW_DOWN && actionState != ActionState.DASH_GRAB;
    }

    public static bool IsGrabbing(this ActionState actionState)
    {
        return actionState == ActionState.GRAB || actionState == ActionState.DASH_GRAB;
    }

    public static bool IsAerialAttack(this ActionState actionState)
    {
        return actionState >= ActionState.AERIAL_ATTACK_START && actionState <= ActionState.AERIAL_ATTACK_END;
    }

    public static bool IsForwardTilt(this ActionState actionState)
    {
        return actionState >= ActionState.ATTACK_FTILT_START && actionState <= ActionState.ATTACK_FTILT_END;
    }

    public static bool IsForwardSmash(this ActionState actionState)
    {
        return actionState >= ActionState.ATTACK_FSMASH_START && actionState <= ActionState.ATTACK_FSMASH_END;
    }

    public static bool IsWavedashInitiationAnimation(this ActionState actionState)
    {
        if (actionState == ActionState.AIR_DODGE)
        {
            return true;
        }

        var isAboveMin = actionState >= ActionState.CONTROLLED_JUMP_START;
        var isBelowMax = actionState <= ActionState.CONTROLLED_JUMP_END;
        return isAboveMin && isBelowMax;
    }

    public static bool IsThrown(this ActionState? actionState)
    {
        return actionState switch
        {
            ActionState.THROWN_BACKWARD or
            ActionState.THROWN_DOWN or
            ActionState.THROWN_DOWN_FEMALE or
            ActionState.THROWN_FORWARD or
            ActionState.THROWN_UP => true,
            _ => false,
        };
    }
}