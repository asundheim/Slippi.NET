using ComboInterpreter;

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

    public abstract void Begin();

    public CancellationToken CancellationToken => _cancellationToken;

    public virtual void Dispose() 
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
