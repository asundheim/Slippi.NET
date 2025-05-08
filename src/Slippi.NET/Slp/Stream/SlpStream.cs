using Slippi.NET.Console;
using Slippi.NET.Slp.Reader;
using Slippi.NET.Slp.Stream.Types;
using Slippi.NET.Types;
using Slippi.NET.Utils;
using System.Text;

namespace Slippi.NET.Slp.Stream;
public class SlpStream
{
    private bool _gameEnded = false;
    private SlpStreamSettings _settings;
    private Dictionary<Command, int>? _payloadSizes = null;
    private byte[] _previousBuffer = [];

    public SlpStream(SlpStreamSettings slpOptions)
    {
        _settings = slpOptions;
    }

    public event EventHandler<SlpStreamCommandEventArgs>? OnCommand;
    public event EventHandler<SlpStreamRawEventArgs>? OnRaw;

    public void Restart()
    {
        _gameEnded = false;
        _payloadSizes = null;
    }

    public void Write(byte[] newData)
    {
        // Join the current data with the old data
        Span<byte> data = [.. _previousBuffer, .. newData];

        // Clear previous data
        _previousBuffer = [];

        SlpDataReader x = new SlpDataReader(data);

        // Iterate through the data
        int index = 0;
        while (index < data.Length)
        {
            if (Encoding.UTF8.GetString(data.Slice(index, 5)) == ConsoleConnection.NETWORK_MESSAGE)
            {
                index += 5;
                continue;
            }

            // Make sure we have enough data to read a full payload
            Command command = x.ReadUInt8(index).EnumCast<Command>() ?? throw new Exception("Failed to parse command from newData");

            int payloadSize = 0;
            _payloadSizes?.TryGetValue(command, out payloadSize);

            int remainingLen = data.Length - index;
            if (remainingLen < payloadSize + 1)
            {
                // If remaining length is not long enough for full payload, save the remaining
                // data until we receive more data. The data has been split up.
                _previousBuffer = data.Slice(index).ToArray();
                break;
            }

            // Only process if the game is still going
            if (_settings.Mode == SlpStreamModes.MANUAL && _gameEnded)
            {
                break;
            }

            // Increment by one for the command byte
            index += 1;

            Span<byte> payloadPtr = data.Slice(index);
            SlpDataReader xPayload = new SlpDataReader(payloadPtr);
            int payloadLen = 0;

            try
            {
                payloadLen = ProcessCommand(command, payloadPtr, xPayload);
            }
            catch (Exception)
            {
                // Only throw the error if we're not suppressing the errors
                if (!_settings.SuppressErrors)
                {
                    throw;
                }

                payloadLen = 0;
            }

            index += payloadLen;
        }
    }

    private byte[] WriteCommand(Command command, Span<byte> entirePayload, int payloadSize)
    {
        Span<byte> payloadBuf = entirePayload.Slice(payloadSize);
        byte[] bufToWrite = [(byte)command, .. payloadBuf];

        // Forward the raw buffer onwards
        OnRaw?.Invoke(this, new SlpStreamRawEventArgs() { Command = command, Payload = bufToWrite });

        return bufToWrite;
    }

    private int ProcessCommand(Command command, Span<byte> entirePayload, SlpDataReader x)
    {
        // Handle the message size command
        if (command == Command.MESSAGE_SIZES)
        {
            byte messagePayloadSize = x.ReadUInt8(0) ?? throw new Exception("Failed to read payloadSize from reader");

            // Set the payload sizes
            _payloadSizes = ProcessReceiveCommands(x);

            // Emit the raw command event
            WriteCommand(command, entirePayload, messagePayloadSize);
            OnCommand?.Invoke(this, new SlpStreamCommandEventArgs() { Command = command, Payload = _payloadSizes });

            return messagePayloadSize;
        }

        int payloadSize = 0;
        if (_payloadSizes is not null)
        {
            _payloadSizes.TryGetValue(command, out payloadSize);
        }

        // Fetch the payload and parse it
        byte[] payload;
        EventPayload? parsedPayload = null;
        if (payloadSize > 0)
        {
            payload = WriteCommand(command, entirePayload, payloadSize);
            parsedPayload = SlpFile.ParseMessage(command, payload.AsSpan());
        }

        if (parsedPayload is null)
        {
            return payloadSize;
        }

        if (command == Command.GAME_END && _settings.Mode == SlpStreamModes.MANUAL)
        {
            // Stop parsing data until we manually restart the stream
            _gameEnded = true;
        }

        OnCommand?.Invoke(this, new SlpStreamCommandEventArgs() { Command = command, Payload = parsedPayload });
        return payloadSize;
    }

    private static Dictionary<Command, int> ProcessReceiveCommands(SlpDataReader x)
    {
        Dictionary<Command, int> payloadSizes = [];
        byte payloadLen = x.ReadUInt8(0) ?? 0;
        for (int i = 1; i < payloadLen; i += 3)
        {
            Command command = x.ReadUInt8(i).EnumCast<Command>() ?? throw new Exception("Failed to parse command from stream");
            ushort payloadSize = x.ReadUInt16(i + 1) ?? 0;

            payloadSizes[command] = payloadSize;
        }

        return payloadSizes;
    }
}
