using Slippi.NET.Common;

namespace Slippi.NET.Console.Types;

public class ConnectionEvent : IEvent<object?>
{
    public required string Event { get; init; }

    public object? Args { get; init; }
}

public abstract class Connection : EventEmitter<ConnectionEvent, object?>, IDisposable
{
    public abstract ConnectionStatus GetStatus();
    public abstract ConnectionSettings GetSettings();
    public abstract ConnectionDetails GetDetails();
    public abstract void Connect(string ip, int port, bool isRealtime, int timeout);
    public abstract void Disconnect();

    public abstract void Dispose();
}
