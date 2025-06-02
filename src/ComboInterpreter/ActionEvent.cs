using Slippi.NET.Stats.Types;
using Slippi.NET.Types;
using System.Diagnostics;

namespace ComboInterpreter;

[DebuggerDisplay("{Action}")]
public record class ActionEvent
{
    public int Frame => FrameEntry.Frame!.Value;
    public required FrameEntry FrameEntry { get; set; }
    public required Actions Action { get; set; }
    public bool HasContinuation { get; set; } = false;
}
