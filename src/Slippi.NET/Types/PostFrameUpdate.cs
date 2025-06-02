namespace Slippi.NET.Types;

public record class PostFrameUpdate : FrameUpdate
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

    public byte? InternalCharacterId { get; set; }
    public float? Percent { get; set; }
    public float? ShieldSize { get; set; }
    public byte? LastAttackLanded { get; set; }
    public byte? CurrentComboCount { get; set; }
    public byte? LastHitBy { get; set; }
    public byte? StocksRemaining { get; set; }
    public float? ActionStateCounter { get; set; }
    public StateFlags1? StateFlags1 { get; set; }
    public StateFlags3? StateFlags3 { get; set; }
    public StateFlags2? StateFlags2 { get; set; }
    public StateFlags4? StateFlags4 { get; set; }
    public StateFlags5? StateFlags5 { get; set; }
    public float? MiscActionState { get; set; }
    public bool? IsAirborne { get; set; }
    public ushort? LastGroundId { get; set; }
    public byte? JumpsRemaining { get; set; }
    public byte? LCancelStatus { get; set; }
    public byte? HurtboxCollisionState { get; set; }
    public SelfInducedSpeeds? SelfInducedSpeeds { get; set; }
    public float? HitlagRemaining { get; set; }
    public uint? AnimationIndex { get; set; }
    public ushort? InstanceHitBy { get; set; }
    public ushort? InstanceId { get; set; }
}