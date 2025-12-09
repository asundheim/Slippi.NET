namespace Slippi.NET.Types;

public delegate bool EventCallbackFunc(Command command, ReadOnlySpan<byte> buffer, EventPayload? payload = null);