namespace Slippi.NET.Stats.Types;

public record class ComboType : DurationType
{
    public required float StartPercent { get; init; }
    public required float CurrentPercent { get; set; }
    public float? EndPercent { get; set; }
    public required int PlayerIndex { get; init; }
    public required IList<MoveLandedType> Moves { get; init; }
    public required bool DidKill { get; set; }
    public int? LastHitBy { get; init; }
}