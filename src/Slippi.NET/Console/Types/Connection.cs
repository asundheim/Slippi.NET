namespace Slippi.NET.Console.Types;

public abstract class Connection : IDisposable
{
    public abstract ConnectionStatus GetStatus();
    public abstract ConnectionSettings GetSettings();
    public abstract ConnectionDetails GetDetails();
    public abstract void Connect(string ip, int port, bool isRealtime, int timeout);
    public abstract void HandleDisconnect();

    public event EventHandler? OnConnect;
    public event EventHandler<Exception>? OnError;
    public event EventHandler<CommunicationMessage>? OnMessage;
    public event EventHandler<ConnectionDetails>? OnHandshake;
    public event EventHandler<ConnectionStatus>? OnStatusChange;
    public event EventHandler<byte[]>? OnData;

    protected void EmitOnConnectEvent() => OnConnect?.Invoke(this, EventArgs.Empty);
    protected void EmitOnErrorEvent(Exception e) => OnError?.Invoke(this, e);
    protected void EmitOnMessageEvent(CommunicationMessage message) => OnMessage?.Invoke(this, message);
    protected void EmitOnHandshakeEvent(ConnectionDetails details) => OnHandshake?.Invoke(this, details);
    protected void EmitOnStatusChangeEvent(ConnectionStatus status) => OnStatusChange?.Invoke(this, status);
    protected void EmitOnDataEvent(byte[] data) => OnData?.Invoke(this, data);

    public abstract void Dispose();
}
