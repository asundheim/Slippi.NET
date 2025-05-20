using System.Runtime.CompilerServices;

namespace Slippi.NET.Utils;

internal static class ByteUtils
{
    public static T? EnumCast<T>(this byte? b) where T : struct, Enum => EnumCast<byte, T>(b);
    public static T? EnumCast<T>(this ushort? u) where T : struct, Enum => EnumCast<ushort, T>(u);
    public static T? EnumCast<T>(this uint? u) where T : struct, Enum => EnumCast<uint, T>(u);

    public static TTo? EnumCast<TFrom, TTo>(TFrom? b) 
        where TFrom : struct 
        where TTo : struct, Enum
    {
        if (b is null)
        {
            return null;
        }

        TFrom bb = (TFrom)b;
        return Unsafe.As<TFrom, TTo>(ref bb);
    }
}
