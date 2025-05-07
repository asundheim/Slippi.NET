namespace Slippi.NET.Types;

public record class PostFrameUpdateType
{
    public PostFrameUpdateType(
        int? frame, 
        int? playerIndex, 
        bool? isFollower, 
        int? internalCharacterId, 
        int? actionStateId, 
        float? positionX, 
        float? positionY, 
        float? facingDirection, 
        float? percent, 
        float? shieldSize, 
        int? lastAttackLanded, 
        int? currentComboCount, 
        int? lastHitBy, 
        int? stocksRemaining, 
        int? actionStateCounter, 
        int? miscActionState, 
        bool? isAirborne, 
        int? lastGroundId, 
        int? jumpsRemaining, 
        int? lCancelStatus, 
        int? hurtboxCollisionState, 
        SelfInducedSpeedsType? selfInducedSpeeds, 
        float? hitlagRemaining, 
        int? animationIndex, 
        int? instanceHitBy, 
        int? instanceId)
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

    public int? Frame { get; set; }
    public int? PlayerIndex { get; set; }
    public bool? IsFollower { get; set; }
    public int? InternalCharacterId { get; set; }
    public int? ActionStateId { get; set; }
    public float? PositionX { get; set; }
    public float? PositionY { get; set; }
    public float? FacingDirection { get; set; }
    public float? Percent { get; set; }
    public float? ShieldSize { get; set; }
    public int? LastAttackLanded { get; set; }
    public int? CurrentComboCount { get; set; }
    public int? LastHitBy { get; set; }
    public int? StocksRemaining { get; set; }
    public int? ActionStateCounter { get; set; }
    public int? MiscActionState { get; set; }
    public bool? IsAirborne { get; set; }
    public int? LastGroundId { get; set; }
    public int? JumpsRemaining { get; set; }
    public int? LCancelStatus { get; set; }
    public int? HurtboxCollisionState { get; set; }
    public SelfInducedSpeedsType? SelfInducedSpeeds { get; set; }
    public float? HitlagRemaining { get; set; }
    public int? AnimationIndex { get; set; }
    public int? InstanceHitBy { get; set; }
    public int? InstanceId { get; set; }
}