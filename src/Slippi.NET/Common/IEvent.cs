namespace Slippi.NET.Common;

public interface IEvent<TEventArgs>
{
    string Event { get; }

    TEventArgs Args { get; }
}
