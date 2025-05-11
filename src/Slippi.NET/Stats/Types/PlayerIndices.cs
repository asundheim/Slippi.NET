namespace Slippi.NET.Stats.Types;

public record class PlayerIndices
{
    public required int PlayerIndex { get; init; }
    public required int OpponentIndex { get; init; }
}
