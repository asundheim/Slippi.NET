namespace Slippi.NET.Stats.Types;

public record class LastEndFrameMetadataType
{
    public LastEndFrameMetadataType()
    {
        LastEndFrameByOppIdx = new Dictionary<int, int>();
    }

    public Dictionary<int, int> LastEndFrameByOppIdx { get; }
}