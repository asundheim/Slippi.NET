using Slippi.NET.Types;

namespace Slippi.NET.Stats.Types;

public record class ComboState
{
    public ComboInfo? Combo { get; set; }
    public MoveLandedInfo? Move { get; set; }
    public required int ResetCounter { get; set; }
    public ActionState? LastHitAnimation { get; set; }
    public string? Event { get; set; }
}
