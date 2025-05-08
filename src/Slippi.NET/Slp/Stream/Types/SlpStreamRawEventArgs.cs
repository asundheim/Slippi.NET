using Slippi.NET.Types;

namespace Slippi.NET.Slp.Stream.Types;

public class SlpStreamRawEventArgs : EventArgs
{
    public required Command Command { get; init; }
    public required byte[] Payload { get; init; }
}
