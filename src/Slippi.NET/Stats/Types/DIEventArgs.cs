using Slippi.NET.Types;

namespace ComboInterpreter;

public record class DIEventArgs
{
    public required int PlayerIndex { get; init; }
    public required PreFrameUpdate PreFrameUpdate { get; init; }
    public required PostFrameUpdate PostFrameUpdate { get; init; }
}
