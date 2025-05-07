namespace Slippi.NET.Types;

public record class FrameEntryType
{
    public FrameEntryType(
        int frame, 
        FrameStartType? start, 
        Dictionary<int, PlayerFrameData?> players, 
        Dictionary<int, PlayerFrameData?> followers, 
        List<ItemUpdateType>? items)
    {
        Frame = frame;
        Start = start;
        Players = players;
        Followers = followers;
        Items = items;
    }

    public int Frame { get; set; }
    public FrameStartType? Start { get; set; }
    public Dictionary<int, PlayerFrameData?> Players { get; set; }
    public Dictionary<int, PlayerFrameData?> Followers { get; set; }
    public List<ItemUpdateType>? Items { get; set; }
}

public record class PlayerFrameData
{
    public PlayerFrameData(PreFrameUpdateType pre, PostFrameUpdateType post)
    {
        Pre = pre;
        Post = post;
    }

    public PreFrameUpdateType Pre { get; set; }
    public PostFrameUpdateType Post { get; set; }
}