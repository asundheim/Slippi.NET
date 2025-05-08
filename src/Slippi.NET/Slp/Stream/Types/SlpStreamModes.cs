
namespace Slippi.NET.Slp.Stream.Types;

public static class SlpStreamModes
{
    public const string AUTO = "AUTO"; // Always reading data, but errors on invalid command
    public const string MANUAL = "MANUAL"; // Stops parsing inputs after a valid game end command, requires manual restarting
}
