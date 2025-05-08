namespace Slippi.NET.Types;

public record class FrameEntry
{
    public int? Frame { get; set; }
    public FrameStart? Start { get; set; }
    public Dictionary<int, PlayerFrameData?>? Players { get; set; }
    public Dictionary<int, PlayerFrameData?>? Followers { get; set; }
    public List<ItemUpdate>? Items { get; set; }
}

public record class PlayerFrameData
{
    public PreFrameUpdate? Pre { get; set; }
    public PostFrameUpdate? Post { get; set; }
}