namespace Slippi.NET.Types;

public record class GameEnd
{
    public GameEnd(GameEndMethod? gameEndMethod, int? lrasInitiatorIndex, List<Placement> placements)
    {
        GameEndMethod = gameEndMethod;
        LrasInitiatorIndex = lrasInitiatorIndex;
        Placements = placements;
    }

    public GameEndMethod? GameEndMethod { get; set; }
    public int? LrasInitiatorIndex { get; set; }
    public List<Placement> Placements { get; set; } = new();
}