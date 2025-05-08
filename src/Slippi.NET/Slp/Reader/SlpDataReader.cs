using static System.Buffers.Binary.BinaryPrimitives;

namespace Slippi.NET.Slp.Reader;

internal ref struct SlpDataReader
{
    private readonly ReadOnlySpan<byte> _buffer;

    public SlpDataReader(ReadOnlySpan<byte> buffer)
    {
        _buffer = buffer;
    }

    public float? ReadFloat(int offset)
    {
        if (ValidateRead(offset, sizeof(float)))
        {
            return ReadSingleBigEndian(_buffer.Slice(offset));
        }

        return null;
    }

    public int? ReadInt32(int offset)
    {
        if (ValidateRead(offset, sizeof(int))) 
        {
            return ReadInt32BigEndian(_buffer.Slice(offset));
        }

        return null;
    }

    public sbyte? ReadInt8(int offset)
    {
        if (ValidateRead(offset, sizeof(sbyte)))
        {
            return (sbyte)_buffer[offset];
        }

        return null;
    }

    public uint? ReadUInt32(int offset)
    {
        if (ValidateRead(offset, sizeof(uint)))
        {
            return ReadUInt32BigEndian(_buffer.Slice(offset));
        }

        return null;
    }

    public ushort? ReadUInt16(int offset)
    {
        if (ValidateRead(offset, sizeof (ushort)))
        { 
            return ReadUInt16BigEndian(_buffer.Slice(offset));
        }

        return null;
    }

    public byte? ReadUInt8(int offset, byte bitmask = 0xff)
    {
        if (ValidateRead(offset, sizeof(byte)))
        {
            return (byte)(_buffer[offset] & bitmask);
        }

        return null;
    }

    public bool? ReadBool(int offset)
    {
        byte? result = ReadUInt8(offset);
        return result is null ? null : (result != 0);
    }

    private bool ValidateRead(int offset, int length) => offset + length <= _buffer.Length;
}
