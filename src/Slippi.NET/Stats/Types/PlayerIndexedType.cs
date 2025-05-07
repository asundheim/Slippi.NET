namespace Slippi.NET.Stats.Types;

public record class PlayerIndexedType
{
    public required int PlayerIndex { get; init; }
    public required int OpponentIndex { get; init; }
}