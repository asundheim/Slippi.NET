namespace Slippi.NET.Console.Types;

public enum ConnectionStatus
{
    Disconnected = 0,
    Connecting = 1,
    Connected = 2,
    ReconnectWait = 3
}