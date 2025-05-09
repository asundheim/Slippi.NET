using System;
using System.IO;
using static System.Buffers.Binary.BinaryPrimitives;

namespace UBJson;
public class BigEndianWriter : BinaryWriter
{
    public BigEndianWriter(Stream output) : base(output)
    {
    }

    public void WriteShort(short value)
    {
        Span<byte> s = stackalloc byte[sizeof(short)];
        WriteInt16BigEndian(s, value);

        Write(s);
    }

    public void WriteUShort(ushort value)
    {
        Span<byte> s = stackalloc byte[sizeof(ushort)];
        WriteUInt16BigEndian(s, value);

        Write(s);
    }

    public void WriteInt(int value)
    {
        Span<byte> s = stackalloc byte[sizeof(int)];
        WriteInt32BigEndian(s, value);

        Write(s);
    }

    public void WriteUInt(uint value)
    {
        Span<byte> s = stackalloc byte[sizeof(uint)];
        WriteUInt32BigEndian(s, value);

        Write(s);
    }

    public void WriteLong(long value)
    {
        Span<byte> s = stackalloc byte[sizeof(long)];
        WriteInt64BigEndian(s, value);

        Write(s);
    }

    public void WriteULong(ulong value)
    {
        Span<byte> s = stackalloc byte[sizeof(ulong)];
        WriteUInt64BigEndian(s, value);

        Write(s);
    }

    public void WriteFloat(float value)
    {
        Span<byte> s = stackalloc byte[sizeof(float)];
        WriteSingleBigEndian(s, value);

        Write(s);
    }

    public void WriteDouble(double value)
    {
        Span<byte> s = stackalloc byte[sizeof(double)];
        WriteDoubleBigEndian(s, value);

        Write(s);
    }
}
