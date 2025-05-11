namespace Slippi.NET.Console.Types;

public record class ConsoleConnectionDetails : ConnectionDetails
{
    /// <summary>
    /// TODO this is currently not implemented.
    /// </summary>
    public bool AutoReconnect { get; set; } = true;
}
