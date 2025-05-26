using Slippi.NET.Stats.Types;
using Slippi.NET.Types;

namespace ComboInterpreter;

public record class ActionEvent
{
    public int Frame => FrameEntry.Frame!.Value;
    public required FrameEntry FrameEntry { get; set; }
    public required Actions Action { get; set; }
    public bool HasContinuation { get; set; } = false;
}
