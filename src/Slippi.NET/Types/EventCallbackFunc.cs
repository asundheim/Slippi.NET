namespace Slippi.NET.Types;

public delegate bool EventCallbackFunc(Command command, EventPayload? payload = null, byte[]? buffer = null);