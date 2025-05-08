namespace Slippi.NET.Types;

public record class MatchInfo
{
    public MatchInfo(string? matchId, uint? gameNumber, uint? tiebreakerNumber)
    {
        MatchId = matchId;
        GameNumber = gameNumber;
        TiebreakerNumber = tiebreakerNumber;
    }

    public string? MatchId { get; set; }
    public uint? GameNumber { get; set; }
    public uint? TiebreakerNumber { get; set; }
}