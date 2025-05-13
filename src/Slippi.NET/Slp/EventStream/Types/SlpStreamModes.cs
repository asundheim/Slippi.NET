namespace Slippi.NET.Slp.EventStream.Types;

/// <summary>
/// Contains a set of valid values for <see cref="SlpStreamSettings.Mode"/>
/// </summary>
public static class SlpStreamModes
{
    /// <summary>
    /// Always reading data, but errors on invalid command
    /// </summary>
    public const string AUTO = "AUTO";

    /// <summary>
    /// Stops parsing inputs after a valid game end command, requires manual restarting
    /// </summary>
    public const string MANUAL = "MANUAL";
}
