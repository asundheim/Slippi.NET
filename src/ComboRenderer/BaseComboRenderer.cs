using ComboInterpreter;
using OBSWebsocketDotNet;
using Slippi.NET.Console.Types;

namespace ComboRenderer;

internal abstract class BaseComboRenderer : IDisposable
{
    protected CancellationTokenSource _cts = new CancellationTokenSource();
    protected CancellationToken _cancellationToken;

    public BaseComboRenderer()
    {
        _cancellationToken = _cts.Token;
    }

    public event EventHandler<FoxComboInterpreter>? OnNewGame;
    protected void InvokeNewGame(FoxComboInterpreter comboBot) => OnNewGame?.Invoke(this, comboBot);

    public event EventHandler? OnGameEnd;
    protected void InvokeGameEnd() => OnGameEnd?.Invoke(this, EventArgs.Empty);

    public event EventHandler<DIEventArgs>? OnDI;
    protected void InvokeDI(DIEventArgs e) => OnDI?.Invoke(this, e);

    public event EventHandler<ConnectionStatus>? OnStatusChange;
    protected void InvokeOnStatusChange(ConnectionStatus s) => OnStatusChange?.Invoke(this, s);

    public abstract void Begin(OBSWebsocket? obs = null);

    public CancellationToken CancellationToken => _cancellationToken;

    public virtual void Dispose() 
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
