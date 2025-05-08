namespace Slippi.NET.Types;

public record class Placement
{
    public required int PlayerIndex { get; init; }
    public required sbyte? Position { get; init; }
}