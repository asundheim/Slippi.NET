namespace Slippi.NET.Common;

// TODO remove this and go back to strongly typed events
public abstract class EventEmitter<TEvent, TEventArgs> where TEvent : IEvent<TEventArgs>
{
    public event EventHandler<TEvent>? OnEvent;

    protected void Emit(TEvent evt)
    {
        OnEvent?.Invoke(this, evt);
    }
}
