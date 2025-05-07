namespace Slippi.NET.Types;

public record class GameEndType
{
    public GameEndType(GameEndMethod? gameEndMethod, int? lrasInitiatorIndex, List<PlacementType> placements)
    {
        GameEndMethod = gameEndMethod;
        LrasInitiatorIndex = lrasInitiatorIndex;
        Placements = placements;
    }

    public GameEndMethod? GameEndMethod { get; set; }
    public int? LrasInitiatorIndex { get; set; }
    public List<PlacementType> Placements { get; set; } = new();
}