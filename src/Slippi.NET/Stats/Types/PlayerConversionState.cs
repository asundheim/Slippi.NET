using Slippi.NET.Stats.Types;

namespace Slippi.NET.Stats.Types;

public record class PlayerConversionState
{
    public required Conversion? Conversion { get; set; }
    public required MoveLandedType? Move { get; set; }
    public required int ResetCounter { get; set; }
    public required int? LastHitAnimation { get; set; }
}