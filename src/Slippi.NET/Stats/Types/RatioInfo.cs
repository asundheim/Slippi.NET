namespace Slippi.NET.Stats.Types;

public record class RatioInfo
{
    public required int Count { get; init; }
    public required int Total { get; init; }
    public double? Ratio { get; init; }
}
