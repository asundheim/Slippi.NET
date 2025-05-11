namespace Slippi.NET.Stats.Types;

public record class LastEndFrameMetadata
{
    public LastEndFrameMetadata()
    {
        LastEndFrameByOppIdx = new Dictionary<int, int>();
    }

    public Dictionary<int, int> LastEndFrameByOppIdx { get; }
}
