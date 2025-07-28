using System.Runtime.InteropServices;

namespace Slippi.NET.Console;

/// <summary>
/// This abomination exists because the .NET ENet library I found did not work, but the native one did.
/// </summary>
internal static partial class DolphinENet
{
    [LibraryImport("DolphinENet.dll")]
    public static partial int Initialize();

    [LibraryImport("DolphinENet.dll")]
    public static partial int Connect([MarshalAs(UnmanagedType.LPUTF8Str)] string pzHost, ushort port);

    [LibraryImport("DolphinENet.dll")]
    public static partial int SendToPeer([In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] buffer, int length);

    [LibraryImport("DolphinENet.dll")]
    public static partial int Read(int timeout, ref int pLength, [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] pData);

    [LibraryImport("DolphinENet.dll")]
    public static partial int Disconnect();

    [LibraryImport("DolphinENet.dll")]
    public static partial int Uninitialize();
}
