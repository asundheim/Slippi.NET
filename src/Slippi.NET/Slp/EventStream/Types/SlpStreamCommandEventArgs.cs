using Slippi.NET.Types;

namespace Slippi.NET.Slp.EventStream.Types;

public class SlpStreamCommandEventArgs : EventArgs
{
    /// <summary>
    /// The event <see cref="Command"/>
    /// </summary>
    public required Command Command { get; init; }

    /// <summary>
    /// The event payload.
    /// </summary>
    /// <remarks>
    /// This is <see cref="EventPayload"/> for all commands except
    /// <see cref="Command.MESSAGE_SIZES"/>, when it is a Dictionary&lt;<see cref="Slippi.NET.Types.Command"/>, <see langword="int"/>&gt;
    /// </remarks>
    public required object Payload { get; init; } // EventPayload or Dictionary<int, int>
}
