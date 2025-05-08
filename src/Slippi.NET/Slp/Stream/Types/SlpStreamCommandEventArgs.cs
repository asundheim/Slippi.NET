using Slippi.NET.Types;

namespace Slippi.NET.Slp.Stream.Types;

public class SlpStreamCommandEventArgs : EventArgs
{
    public required Command Command { get; init; }
    public required object Payload { get; init; } // EventPayload or Dictionary<int, int>
}
