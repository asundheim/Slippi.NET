using Slippi.NET.Console.Types;
using System.Buffers.Binary;
using UBJson;

namespace Slippi.NET.Console;

// This class is responsible for handling the communication protocol between the Wii and the
// desktop app
public class ConsoleCommunication
{
    private byte[] _receiveBuf;
    private IList<CommunicationMessage> _messages;

    public ConsoleCommunication()
    {
        _receiveBuf = [];
        _messages = [];
    }

    public unsafe void Receive(byte[] data)
    {
        //.._receiveBuf, ..data;
        Span<byte> buffer = stackalloc byte[_receiveBuf.Length + data.Length];
        _receiveBuf.AsSpan().CopyTo(buffer);
        data.AsSpan().CopyTo(buffer.Slice(data.Length));

        while (buffer.Length >= 4)
        {
            // First get the size of the message we are expecting
            uint msgSize = BinaryPrimitives.ReadUInt32BigEndian(_receiveBuf);

            if (buffer.Length < msgSize + 4)
            {
                // If we haven't received all the data yet, let's wait for more
                _receiveBuf = buffer.ToArray();
                return;
            }

            // Here we have received all the data, so let's decode it
            ReadOnlySpan<byte> ubjsonData = buffer.Slice(4, buffer.Length - ((int)msgSize + 4));

            fixed (byte* pUbjsonData = &ubjsonData[0])
            {
                using UnmanagedMemoryStream stream = new UnmanagedMemoryStream(pUbjsonData, ubjsonData.Length);

                _messages.Add(UBJsonReader.Parse<CommunicationMessage>(stream));
            }

            buffer = buffer.Slice((int)msgSize + 4).ToArray();
        }

        _receiveBuf = buffer.ToArray();
    }

    public byte[] GetReceiveBuffer()
    {
        return _receiveBuf.ToArray();
    }

    public IList<CommunicationMessage> GetMessages()
    {
        var toReturn = _messages;
        _messages = [];

        return toReturn;
    }

    public static byte[] GenHandshakeOut(byte[] cursor, int clientToken, bool isRealtime = false)
    {
        Span<byte> clientTokenBuf = [0, 0, 0, 0];
        BinaryPrimitives.WriteUInt32BigEndian(clientTokenBuf, (uint)clientToken);

        var message = new
        {
            type = CommunicationType.Handshake,
            payload = new
            {
                cursor = cursor,
                clientToken = clientTokenBuf.ToArray(),
                isRealtime = isRealtime,
            }
        };

        byte[] buf = UBJsonWriter.Encode(message);

        Span<byte> msg = [0, 0, 0, 0, .. buf];
        BinaryPrimitives.WriteUInt32BigEndian(msg, (uint)buf.Length);

        return msg.ToArray();
    }
}
