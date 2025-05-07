namespace Slippi.NET.Types;

public record class ItemUpdateType
{
    public ItemUpdateType(
        int? frame, 
        int? typeId, 
        int? state, 
        float? facingDirection, 
        float? velocityX, 
        float? velocityY, 
        float? positionX, 
        float? positionY, 
        float? damageTaken, 
        float? expirationTimer, 
        int? spawnId, 
        int? missileType,
        int? turnipFace, 
        int? chargeShotLaunched, 
        float? chargePower, 
        int? owner, 
        int? instanceId)
    {
        Frame = frame;
        TypeId = typeId;
        State = state;
        FacingDirection = facingDirection;
        VelocityX = velocityX;
        VelocityY = velocityY;
        PositionX = positionX;
        PositionY = positionY;
        DamageTaken = damageTaken;
        ExpirationTimer = expirationTimer;
        SpawnId = spawnId;
        MissileType = missileType;
        TurnipFace = turnipFace;
        ChargeShotLaunched = chargeShotLaunched;
        ChargePower = chargePower;
        Owner = owner;
        InstanceId = instanceId;
    }

    public int? Frame { get; set; }
    public int? TypeId { get; set; }
    public int? State { get; set; }
    public float? FacingDirection { get; set; }
    public float? VelocityX { get; set; }
    public float? VelocityY { get; set; }
    public float? PositionX { get; set; }
    public float? PositionY { get; set; }
    public float? DamageTaken { get; set; }
    public float? ExpirationTimer { get; set; }
    public int? SpawnId { get; set; }
    public int? MissileType { get; set; }
    public int? TurnipFace { get; set; }
    public int? ChargeShotLaunched { get; set; }
    public float? ChargePower { get; set; }
    public int? Owner { get; set; }
    public int? InstanceId { get; set; }
}