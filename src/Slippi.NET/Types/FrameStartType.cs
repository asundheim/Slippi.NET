namespace Slippi.NET.Types;

public record class FrameStartType
{
    public FrameStartType(int? frame, int? seed, int? sceneFrameCounter)
    {
        Frame = frame;
        Seed = seed;
        SceneFrameCounter = sceneFrameCounter;
    }

    public int? Frame { get; set; }
    public int? Seed { get; set; }
    public int? SceneFrameCounter { get; set; }
}