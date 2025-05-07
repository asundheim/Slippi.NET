namespace Slippi.NET.Stats.Types;

public record class DurationType
{
    public required int StartFrame { get; set; }
    public int? EndFrame { get; set; }
}