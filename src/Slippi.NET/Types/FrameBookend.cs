namespace Slippi.NET.Types;

public record class FrameBookend
{
    public FrameBookend(int? frame, int? latestFinalizedFrame)
    {
        Frame = frame;
        LatestFinalizedFrame = latestFinalizedFrame;
    }

    public int? Frame { get; set; }
    public int? LatestFinalizedFrame { get; set; }
}