namespace Slippi.NET.Types;

public record class PostFrameUpdate
{
    public PostFrameUpdate(
        int? frame, 
        byte? playerIndex, 
        bool? isFollower, 
        byte? internalCharacterId, 
        ActionState? actionStateId, 
        float? positionX, 
        float? positionY, 
        float? facingDirection, 
        float? percent, 
        float? shieldSize, 
        byte? lastAttackLanded, 
        byte? currentComboCount, 
        byte? lastHitBy, 
        byte? stocksRemaining, 
        float? actionStateCounter, 
        StateFlags1? stateFlags1,
        StateFlags3? stateFlags3,
        StateFlags2? stateFlags2,
        StateFlags4? stateFlags4,
        StateFlags5? stateFlags5,
        float? miscActionState, 
        bool? isAirborne, 
        ushort? lastGroundId, 
        byte? jumpsRemaining, 
        byte? lCancelStatus, 
        byte? hurtboxCollisionState, 
        SelfInducedSpeeds? selfInducedSpeeds, 
        float? hitlagRemaining, 
        uint? animationIndex, 
        ushort? instanceHitBy, 
        ushort? instanceId)
    {
        Frame = frame;
        PlayerIndex = playerIndex;
        IsFollower = isFollower;
        InternalCharacterId = internalCharacterId;
        ActionStateId = actionStateId;
        PositionX = positionX;
        PositionY = positionY;
        FacingDirection = facingDirection;
        Percent = percent;
        ShieldSize = shieldSize;
        LastAttackLanded = lastAttackLanded;
        CurrentComboCount = currentComboCount;
        LastHitBy = lastHitBy;
        StocksRemaining = stocksRemaining;
        ActionStateCounter = actionStateCounter;
        StateFlags1 = stateFlags1;
        StateFlags2 = stateFlags2;
        StateFlags3 = stateFlags3;
        StateFlags4 = stateFlags4;
        StateFlags5 = stateFlags5;
        MiscActionState = miscActionState;
        IsAirborne = isAirborne;
        LastGroundId = lastGroundId;
        JumpsRemaining = jumpsRemaining;
        LCancelStatus = lCancelStatus;
        HurtboxCollisionState = hurtboxCollisionState;
        SelfInducedSpeeds = selfInducedSpeeds;
        HitlagRemaining = hitlagRemaining;
        AnimationIndex = animationIndex;
        InstanceHitBy = instanceHitBy;
        InstanceId = instanceId;
    }

    public int? Frame;
    public byte? PlayerIndex;
    public bool? IsFollower;
    public ActionState? ActionStateId;
    public float? PositionX;
    public float? PositionY;
    public float? FacingDirection;
    public byte? InternalCharacterId;
    public float? Percent;
    public float? ShieldSize;
    public byte? LastAttackLanded;
    public byte? CurrentComboCount;
    public byte? LastHitBy;
    public byte? StocksRemaining;
    public float? ActionStateCounter;
    public StateFlags1? StateFlags1;
    public StateFlags3? StateFlags3;
    public StateFlags2? StateFlags2;
    public StateFlags4? StateFlags4;
    public StateFlags5? StateFlags5;
    public float? MiscActionState;
    public bool? IsAirborne;
    public ushort? LastGroundId;
    public byte? JumpsRemaining;
    public byte? LCancelStatus;
    public byte? HurtboxCollisionState;
    public SelfInducedSpeeds? SelfInducedSpeeds;
    public float? HitlagRemaining;
    public uint? AnimationIndex;
    public ushort? InstanceHitBy;
    public ushort? InstanceId;
}