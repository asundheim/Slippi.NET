namespace Slippi.NET.Types;

public record class FrameUpdate
{
    public int? Frame { get; set; }
    public byte? PlayerIndex { get; set; }
    public bool? IsFollower { get; set; }
    public ushort? ActionStateId { get; set; }
    public float? PositionX { get; set; }
    public float? PositionY { get; set; }
    public float? FacingDirection { get; set; }
}
