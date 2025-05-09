namespace Slippi.NET.Stats.Types;

public record class PlayerActionState
{
    public required ActionCounts PlayerCounts { get; init; }
    public required List<int> Animations { get; init; }
    public required List<float> ActionFrameCounters { get; init; }
}