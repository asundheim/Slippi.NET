namespace Slippi.NET.Common;

public abstract class EventEmitter<TEvent, TEventArgs> where TEvent : IEvent<TEventArgs>
{
    public event EventHandler<TEvent>? OnEvent;

    protected void Emit(TEvent evt)
    {
        OnEvent?.Invoke(this, evt);
    }
}
