namespace Slippi.NET.Console.Types;

public record class ConnectionDetails
{
    public required string ConsoleNick { get; set; }
    public required object GameDataCursor { get; set; } // int or byte[]
    public required string Version { get; set; }
    public uint? ClientToken { get; set; }
}
