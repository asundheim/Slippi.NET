namespace Slippi.NET.Stats.Types;

public record class Stock : DurationType
{
    public required float StartPercent { get; init; }
    public required float CurrentPercent { get; set; }
    public float? EndPercent { get; set; }
    public required int PlayerIndex { get; init; }
    public required int Count { get; init; }
    public int? DeathAnimation { get; set; }
}