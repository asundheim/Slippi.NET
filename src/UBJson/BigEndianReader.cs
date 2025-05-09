using System;
using System.IO;
using System.Text;
using static System.Buffers.Binary.BinaryPrimitives;

namespace UBJson;

public class BigEndianReader
{
    private readonly Stream _stream;
    public BigEndianReader(Stream stream)
    {
        _stream = stream;
    }

    public sbyte ReadInt8()
    {
        Span<byte> s = stackalloc byte[sizeof(short)];
        _stream.ReadExactly(s);

        return (sbyte)s[0];
    }

    public byte ReadUInt8()
    {
        Span<byte> s = stackalloc byte[sizeof(byte)];
        _stream.ReadExactly(s);

        return s[0];
    }

    public short ReadInt16()
    {
        Span<byte> s = stackalloc byte[sizeof(short)];
        _stream.ReadExactly(s);

        return ReadInt16BigEndian(s);
    }

    public char ReadChar() => (char)ReadUInt16();

    public int ReadInt32()
    {
        Span<byte> s = stackalloc byte[sizeof(int)];
        _stream.ReadExactly(s);

        return ReadInt32BigEndian(s);
    }

    public long ReadInt64()
    {
        Span<byte> s = stackalloc byte[sizeof(long)];
        _stream.ReadExactly(s);

        return ReadInt64BigEndian(s);
    }

    public ushort ReadUInt16()
    {
        Span<byte> s = stackalloc byte[sizeof(ushort)];
        _stream.ReadExactly(s);

        return ReadUInt16BigEndian(s);
    }

    public uint ReadUInt32()
    {
        Span<byte> s = stackalloc byte[sizeof(uint)];
        _stream.ReadExactly(s);

        return ReadUInt32BigEndian(s);
    }

    public ulong ReadUInt64()
    {
        Span<byte> s = stackalloc byte[sizeof(ulong)];
        _stream.ReadExactly(s);

        return ReadUInt64BigEndian(s);
    }

    public float ReadSingle()
    {
        Span<byte> s = stackalloc byte[sizeof(float)];
        _stream.ReadExactly(s);

        return ReadSingleBigEndian(s);
    }

    public double ReadDouble()
    {
        Span<byte> s = stackalloc byte[sizeof(double)];
        _stream.ReadExactly(s);

        return ReadDoubleBigEndian(s);
    }

    public string ReadStringUTF8(long length)
    {
        byte[] s = new byte[length];
        _stream.ReadExactly(s, 0, (int)length);

        return Encoding.UTF8.GetString(s);
    }

    public long Position => _stream.Position;
    public long Length => _stream.Length;
}
