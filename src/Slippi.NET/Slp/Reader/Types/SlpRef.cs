using Slippi.NET.Types;
using System.Buffers.Binary;

namespace Slippi.NET.Slp.Reader.Types;

public abstract class SlpRef
{
    public abstract string Source { get; }

    public abstract int ReadRef(Span<byte> buffer, int position);
    public abstract int GetLenRef();

    public int GetRawDataPosition()
    {
        Span<byte> buffer = stackalloc byte[1];
        int bytesRead = ReadRef(buffer, position: 0);
        // ignoring a check / slice here as it's assumed reading < 1 bytes and leaving 0
        // in the buffer is intentional behaviour

        if (buffer[0] == 0x36)
        {
            return 0;
        }
        else if (buffer[0] != '{')
        {
            return 0; // return error? (jlaferri, 7 years ago - "first real commit")
        }
        else
        {
            return 15;
        }
    }

    public int GetRawDataLength(int position)
    {
        int fileSize = GetLenRef();
        if (position == 0)
        {
            return fileSize;
        }

        Span<byte> buffer = stackalloc byte[4];
        int bytesRead = ReadRef(buffer, position: position - 4);
        // ignoring a check / slice here as it's assumed reading < 4 bytes and leaving 0
        // in the buffer is intentional behaviour

        int rawDataLen = buffer[0] << 24 | buffer[1] << 16 | buffer[2] << 8 | buffer[3];
        if (rawDataLen > 0)
        {
            // If this method manages to read a number, it's probably trustworthy
            return rawDataLen;
        }

        // If the above does not return a valid data length,
        // return a file size based on file length. This enables
        // some support for severed files
        return fileSize - position;
    }

    public int GetMetadataLength(int position)
    {
        int len = GetLenRef();

        return len - position - 1;
    }

    public Dictionary<int, int> GetMessageSizes(int position)
    {
        Dictionary<int, int> messageSizes = [];

        // Support old file format
        if (position == 0)
        {
            messageSizes[0x36] = 0x140;
            messageSizes[0x37] = 0x6;
            messageSizes[0x38] = 0x46;
            messageSizes[0x39] = 0x1;

            return messageSizes;
        }

        Span<byte> buffer = stackalloc byte[2];
        ReadRef(buffer, position);
        // ignoring a check / slice here as it's assumed reading < 2 bytes and leaving 0
        // in the buffer is intentional behaviour

        if (buffer[0] != (byte)Command.MESSAGE_SIZES)
        {
            return messageSizes;
        }

        int payloadLength = buffer[1];
        messageSizes[0x35] = payloadLength;

        Span<byte> messageSizesBuffer = stackalloc byte[payloadLength - 1];
        ReadRef(messageSizesBuffer, position + 2);
        // ignoring a check / slice here as it's assumed reading < 2 bytes and leaving 0
        // in the buffer is intentional behaviour

        for (int i = 0; i < payloadLength - 1; i += 3)
        {
            byte command = messageSizesBuffer[i];

            // Get size of command
            messageSizes[command] = BinaryPrimitives.ReadUInt16BigEndian(messageSizesBuffer.Slice(i + 1, 2));
        }

        return messageSizes;
    }
}
