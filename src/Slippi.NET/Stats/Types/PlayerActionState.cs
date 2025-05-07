namespace Slippi.NET.Stats.Types;

public record class PlayerActionState
{
    public required ActionCountsType PlayerCounts { get; init; }
    public required List<int> Animations { get; init; }
    public required List<int> ActionFrameCounters { get; init; }
}