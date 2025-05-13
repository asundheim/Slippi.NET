using Slippi.NET.Types;
using Slippi.NET.Slp.Parser;

namespace Slippi.NET.Slp.EventStream.Types;

public class SlpStreamRawEventArgs : EventArgs
{
    /// <summary>
    /// The command associated to the raw payload bytes.
    /// </summary>
    public required Command Command { get; init; }

    /// <summary>
    /// The raw payload bytes.
    /// </summary>
    /// <remarks>
    /// This can be interpreted by an <see cref="SlpParser"/> to decode events into objects.
    /// </remarks>
    public required byte[] Payload { get; init; }
}
