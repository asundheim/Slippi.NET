namespace Slippi.NET.Types;

public record class RollbackFrames
{
    public required RollbackFramesCollection Frames { get; set; }
    public required int Count { get; set; }
    public required List<int> Lengths { get; set; }
}
