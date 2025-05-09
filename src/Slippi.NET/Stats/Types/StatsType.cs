namespace Slippi.NET.Stats.Types;

public record class StatsType
{
    public required bool GameComplete { get; init; }
    public required int LastFrame { get; init; }
    public required int PlayableFrameCount { get; init; }
    public required IList<Stock> Stocks { get; init; }
    public required IList<Conversion> Conversions { get; init; }
    public required IList<ComboType> Combos { get; init; }
    public required IList<ActionCounts> ActionCounts { get; init; }
    public required IList<OverallType> Overall { get; init; }
}