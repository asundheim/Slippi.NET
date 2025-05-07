namespace Slippi.NET.Stats.Types;

public record class DamageType
{
    public required float StartPercent { get; init; }
    public required float CurrentPercent { get; init; }
    public float? EndPercent { get; init; }
}