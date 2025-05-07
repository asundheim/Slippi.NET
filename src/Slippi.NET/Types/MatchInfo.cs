namespace Slippi.NET.Types;

public record class MatchInfo
{
    public MatchInfo(string? matchId, int? gameNumber, int? tiebreakerNumber)
    {
        MatchId = matchId;
        GameNumber = gameNumber;
        TiebreakerNumber = tiebreakerNumber;
    }

    public string? MatchId { get; set; }
    public int? GameNumber { get; set; }
    public int? TiebreakerNumber { get; set; }
}