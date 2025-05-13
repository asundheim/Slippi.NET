using System.Runtime.InteropServices;

namespace Slippi.NET.Console;

/// <summary>
/// This abomination exists because the .NET ENet library I found did not work, but the native one did.
/// </summary>
internal static class DolphinENet
{
    [DllImport("DolphinENet.dll")]
    public static extern int Initialize();

    [DllImport("DolphinENet.dll")]
    public static extern int Connect(byte[] pzHost, ushort port);

    [DllImport("DolphinENet.dll")]
    public static extern int SendToPeer(byte[] buffer, int length);

    [DllImport("DolphinENet.dll")]
    public static extern int Read(int timeout, ref int pLength, byte[] pData);

    [DllImport("DolphinENet.dll")]
    public static extern int Disconnect();

    [DllImport("DolphinENet.dll")]
    public static extern int Uninitialize();
}
