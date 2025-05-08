using System.Runtime.CompilerServices;

namespace Slippi.NET.Utils;

internal static class ByteUtils
{
    public static T? EnumCast<T>(this byte? b) where T : struct, Enum
    {
        if (b is null)
        {
            return null;
        }

        byte bb = (byte)b;
        return Unsafe.As<byte, T>(ref bb);
    }
}
