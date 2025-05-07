namespace Slippi.NET.Types;

public record class FrameBookendType
{
    public FrameBookendType(int? frame, int? latestFinalizedFrame)
    {
        Frame = frame;
        LatestFinalizedFrame = latestFinalizedFrame;
    }

    public int? Frame { get; set; }
    public int? LatestFinalizedFrame { get; set; }
}