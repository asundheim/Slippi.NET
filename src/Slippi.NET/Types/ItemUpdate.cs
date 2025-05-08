namespace Slippi.NET.Types;

public record class ItemUpdate
{
    public ItemUpdate(
        int? frame, 
        ushort? typeId, 
        byte? state, 
        float? facingDirection, 
        float? velocityX, 
        float? velocityY, 
        float? positionX, 
        float? positionY, 
        ushort? damageTaken, 
        float? expirationTimer, 
        uint? spawnId, 
        byte? missileType,
        byte? turnipFace, 
        byte? chargeShotLaunched, 
        byte? chargePower, 
        sbyte? owner, 
        ushort? instanceId)
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
    public ushort? TypeId { get; set; }
    public byte? State { get; set; }
    public float? FacingDirection { get; set; }
    public float? VelocityX { get; set; }
    public float? VelocityY { get; set; }
    public float? PositionX { get; set; }
    public float? PositionY { get; set; }
    public ushort? DamageTaken { get; set; }
    public float? ExpirationTimer { get; set; }
    public uint? SpawnId { get; set; }
    public byte? MissileType { get; set; }
    public byte? TurnipFace { get; set; }
    public byte? ChargeShotLaunched { get; set; }
    public byte? ChargePower { get; set; }
    public sbyte? Owner { get; set; }
    public ushort? InstanceId { get; set; }
}