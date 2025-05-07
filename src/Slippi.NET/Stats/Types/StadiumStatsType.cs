namespace Slippi.NET.Stats.Types;

public abstract record class StadiumStatsType
{
    public required string Type { get; init; }
}

public record class TargetTestResultType : StadiumStatsType
{
    public required IList<TargetBreakType> TargetBreaks { get; init; }

    public TargetTestResultType()
    {
        Type = "target-test";
    }
}

public record class HomeRunContestResultType : StadiumStatsType
{
    public required float Distance { get; init; }
    public required string Units { get; init; }

    public HomeRunContestResultType()
    {
        Type = "home-run-contest";
    }
}