namespace Slippi.NET.Console.Types;

public record class ConsoleConnectionDetails : ConnectionDetails
{
    public bool AutoReconnect { get; set; } = true;
}
