namespace Slippi.NET.Stats.Types;

public record class MoveLandedType
{
    public required int PlayerIndex { get; init; }
    public required int Frame { get; init; }
    public required int MoveId { get; init; }
    public required int HitCount { get; set; }
    public required float Damage { get; set; }
}