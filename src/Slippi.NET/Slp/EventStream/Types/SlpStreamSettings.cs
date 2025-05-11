using static Slippi.NET.Slp.EventStream.Types.SlpStreamModes;

namespace Slippi.NET.Slp.EventStream.Types;

public record class SlpStreamSettings
{
    /// <summary>
    /// [Optional] If errors should be suppressed or if they should be thrown as exceptions.
    /// </summary>
    public bool SuppressErrors { get; set; } = false;

    /// <summary>
    /// Indicates how the stream should read events.
    /// Valid values: <br/>
    /// <see cref="SlpStreamModes.AUTO"/> (Default) - Always reading data, but errors on invalid command <br/>
    /// <see cref="SlpStreamModes.MANUAL"/> - Stops parsing inputs after a valid game end command, requires manual restarting
    /// </summary>
    public string Mode { get; set; } = AUTO;
}
