using static Slippi.NET.Slp.Stream.Types.SlpStreamModes;

namespace Slippi.NET.Slp.Stream.Types;

public record class SlpStreamSettings
{
    public bool SuppressErrors { get; set; } = false;
    public string Mode { get; set; } = AUTO;
}
