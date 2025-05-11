namespace Slippi.NET.Stats.Types;

public record class DurationInfo
{
    public required int StartFrame { get; set; }
    public int? EndFrame { get; set; }
}
