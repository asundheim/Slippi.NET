namespace Slippi.NET.Stats.Types;

public record class InputCountsType
{
    public required int Buttons { get; init; }
    public required int Triggers { get; init; }
    public required int Joystick { get; init; }
    public required int CStick { get; init; }
    public required int Total { get; init; }
}