namespace Slippi.NET.Types;

public delegate bool EventCallbackFunc(Command command, EventPayloadTypes? payload = null, byte[]? buffer = null);