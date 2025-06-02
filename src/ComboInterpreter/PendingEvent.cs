using System.Diagnostics;

namespace ComboInterpreter;

[DebuggerDisplay("{Action.Actions}")]
public record class PendingAction
{
    public required ActionEvent Action { get; set; }
    public int FramesLeft { get; set; } = -1;
    public int ActionsLeft { get; set; } = -1;

    public Func<ActionEvent, bool>? CancelIf { get; set; } = null;
    public Func<ActionEvent, bool>? ContinuationIf { get; set; } = null;
    public Func<ActionEvent, bool>? FlushIf { get; set; } = null;

    public Func<ActionEvent, bool>? AppendContinuationWithIf { get; set; } = null;
    public ActionEvent? AppendContinuationWith { get; set; } = null;
}
