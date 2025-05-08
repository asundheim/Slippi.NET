namespace Slippi.NET.Types;

public record class SelfInducedSpeeds
{
    public SelfInducedSpeeds(float? airX, float? y, float? attackX, float? attackY, float? groundX)
    {
        AirX = airX;
        Y = y;
        AttackX = attackX;
        AttackY = attackY;
        GroundX = groundX;
    }

    public float? AirX { get; set; }
    public float? Y { get; set; }
    public float? AttackX { get; set; }
    public float? AttackY { get; set; }
    public float? GroundX { get; set; }
}
