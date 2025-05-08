namespace Slippi.NET.Stats.Types;

public record class TargetBreakType
{
    public required uint SpawnId { get; init; }
    public int? FrameDestroyed { get; set; }
    public required float PositionX { get; init; }
    public required float PositionY { get; init; }
}
