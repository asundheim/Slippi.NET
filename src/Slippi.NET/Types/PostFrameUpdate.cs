namespace Slippi.NET.Types;

public record class PostFrameUpdate : FrameUpdate
{
    public PostFrameUpdate(
        int? frame, 
        byte? playerIndex, 
        bool? isFollower, 
        byte? internalCharacterId, 
        ushort? actionStateId, 
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