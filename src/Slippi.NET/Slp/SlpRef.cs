using Slippi.NET.Types;
using Slippi.NET.Slp.Reader.File;
using Slippi.NET.Slp.Reader.Buffer;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Slippi.NET.Slp;

/// <summary>
/// An abstract view of an Slp file that can parse the top level attributes of the file
/// (RawData offset / length, Metadata offset / length, command buffer manifest).
/// </summary>
/// <remarks>
/// Implementations include <see cref="SlpFileRef"/> and <see cref="SlpBufferRef"/>
/// which can be obtained with <see cref="SlpFileReader"/> and <see cref="SlpBufferReader"/>, respectively.
/// </remarks>
public abstract class SlpRef : IDisposable
{
    public abstract string Source { get; }

    public abstract int ReadRef(Span<byte> buffer, int position);
    public abstract int GetLenRef();

    public int GetRawDataPosition()
    {
        Span<byte> buffer = stackalloc byte[1];
        int bytesRead = ReadRef(buffer, position: 0);

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

    public Dictionary<Command, int> GetMessageSizes(int position)
    {
        Dictionary<Command, int> messageSizes = [];

        // Support old file format
        if (position == 0)
        {
            messageSizes[Command.GAME_START] = 0x140;
            messageSizes[Command.PRE_FRAME_UPDATE] = 0x6;
            messageSizes[Command.POST_FRAME_UPDATE] = 0x46;
            messageSizes[Command.GAME_END] = 0x1;

            return messageSizes;
        }

        Span<byte> buffer = stackalloc byte[2];
        int bytesRead = ReadRef(buffer, position);
        Debug.Assert(bytesRead == buffer.Length, "Mismatched read?");

        // 0x0: Command Byte
        if (buffer[0] != (byte)Command.MESSAGE_SIZES)
        {
            Debug.Fail("Message sizes not at requested location?");
            return messageSizes;
        }

        // 0x1: Payload Size - The size in bytes of the payload for this event, including this byte (i.e. 3n+1, where n is the number of commands to follow)
        int payloadLength = buffer[1];
        messageSizes[Command.MESSAGE_SIZES] = payloadLength;

        Span<byte> messageSizesBuffer = stackalloc byte[payloadLength - 1];
        bytesRead = ReadRef(messageSizesBuffer, position + 2);
        Debug.Assert(bytesRead == messageSizesBuffer.Length, "Mismatched read?");

        for (int i = 0; i < payloadLength - 1; i += 3)
        {
            // 0x2 + 0x3i: Command Byte
            Command command = Unsafe.As<byte, Command>(ref messageSizesBuffer[i]);
            Debug.Assert(Enum.IsDefined(command), "Read unknown command?");

            // 0x3 + 0x3i: Command payload size
            messageSizes[command] = BinaryPrimitives.ReadUInt16BigEndian(messageSizesBuffer.Slice(i + 1, 2));
        }

        return messageSizes;
    }

    public abstract void Dispose();
}
