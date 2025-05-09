using Slippi.NET.Types;

namespace Slippi.NET.Stats.Types;

public abstract record class StadiumStats
{
    public abstract string Type { get; }
}

public record class TargetTestStats : StadiumStats
{
    public required IList<TargetBreakType> TargetBreaks { get; init; }

    public override string Type => "target-test";
}

public record class HomeRunContestStats : StadiumStats
{
    public required HomeRunDistanceInfo DistanceInfo { get; init; }

    public override string Type => "home-run-contest";
}
