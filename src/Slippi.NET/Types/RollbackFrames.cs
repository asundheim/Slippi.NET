namespace Slippi.NET.Types;

public record class RollbackFrames
{
    public RollbackFrames(RollbackFrames frames, int count, List<int> lengths)
    {
        Frames = frames;
        Count = count;
        Lengths = lengths;
    }

    public RollbackFrames Frames { get; set; }
    public int Count { get; set; }
    public List<int> Lengths { get; set; }
}
