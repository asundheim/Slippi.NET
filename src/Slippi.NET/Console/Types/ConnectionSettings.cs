namespace Slippi.NET.Console.Types;

public record class ConnectionSettings
{
    public required string IpAddress { get; init; }
    public required int Port { get; init; }
}