namespace Slippi.NET.Stats.Types;

public record class PlayerInput
{
    public required int PlayerIndex { get; init; }
    public required int OpponentIndex { get; init; }
    public required int InputCount { get; set; }
    public required int JoystickInputCount { get; set; }
    public required int CstickInputCount { get; set; }
    public required int ButtonInputCount { get; set; }
    public required int TriggerInputCount { get; set; }
}