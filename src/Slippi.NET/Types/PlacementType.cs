namespace Slippi.NET.Types;

public record class PlacementType
{
    public required int PlayerIndex { get; init; }
    public required int? Position { get; init; }
}