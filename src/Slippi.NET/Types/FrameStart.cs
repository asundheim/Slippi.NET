namespace Slippi.NET.Types;

public record class FrameStart
{
    public FrameStart(int? frame, uint? seed, uint? sceneFrameCounter)
    {
        Frame = frame;
        Seed = seed;
        SceneFrameCounter = sceneFrameCounter;
    }

    public int? Frame { get; set; }
    public uint? Seed { get; set; }
    public uint? SceneFrameCounter { get; set; }
}