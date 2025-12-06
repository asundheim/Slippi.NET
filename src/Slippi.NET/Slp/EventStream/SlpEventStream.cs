using Slippi.NET.Slp.Reader;
using Slippi.NET.Slp.EventStream.Types;
using Slippi.NET.Types;
using Slippi.NET.Utils;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Slippi.NET.Slp.EventStream;

/// <summary>
/// <see cref="SlpEventStream"/> is a writable stream of Slippi data. It parses the data being written in
/// and emits an event based on what kind of Slippi messages were processed. <br/><br/>
///
/// <see cref="SlpEventStream"/> emits two events: <see cref="OnRaw"/> and <see cref="OnCommand"/>. <br/><br/>
/// 
/// The <see cref="OnRaw"/> event emits the raw buffer
/// bytes whenever it processes each command. You can manually parse this or write it to a
/// file. <br/>
/// 
/// The <see cref="OnCommand"/> event returns the parsed payload which you can access the parsed attributes from.
/// </summary>
public class SlpEventStream
{
    private bool _gameEnded = false;
    private readonly SlpStreamSettings _settings;
    private Dictionary<Command, int>? _payloadSizes = null;
    private byte[] _previousBuffer = [];
    private byte[] _splitBuffer = [];

    public SlpEventStream(SlpStreamSettings? settings)
    {
        _settings = settings ?? new SlpStreamSettings();
    }

    public event EventHandler<SlpStreamCommandEventArgs>? OnCommand;
    public event EventHandler<SlpStreamRawEventArgs>? OnRaw;

    public void Restart()
    {
        _gameEnded = false;
        _payloadSizes = null;
    }

    public void Write(in ReadOnlySpan<byte> newData)
    {
        // Join the current data with the old data
        Span<byte> data = [.. _previousBuffer, .. newData];

        // Clear previous data
        _previousBuffer = [];

        BufferReader x = new BufferReader(data);

        // Iterate through the data
        int index = 0;
        while (index < data.Length)
        {
            // Only process if the game is still going
            if (_settings.Mode == SlpStreamModes.MANUAL && _gameEnded)
            {
                break;
            }

            // Filter console HELO
            if ((data.Length - index) >= 5 && data.Slice(index, 5).SequenceEqual([(byte)'H', (byte)'E', (byte)'L', (byte)'O', (byte)'\0']))
            {
                index += 5;
                continue;
            }

            Command command = x.ReadUInt8(index).EnumCast<Command>()!.Value; // We should have at least one byte by the loop condition
            if (_payloadSizes is null)
            {
                if (command == Command.MESSAGE_SIZES)
                {
                    // Now peek at the size byte - if we don't have the full payload, we need to buffer it
                    if (x.ReadUInt8(index + 1) is byte messageSizeLength && (data.Length - index) >= messageSizeLength + 1)
                    {
                        Span<byte> messageSizePayload = data.Slice(index, messageSizeLength + 1);

                        try
                        {
                            ProcessMessageSizesCommand(messageSizePayload);
                        }
                        catch (Exception)
                        {
                            // Reset payload sizes
                            _payloadSizes = null;

                            if (!_settings.SuppressErrors)
                            {
                                throw;
                            }
                        }

                        index += messageSizePayload.Length; // 1 + actual payload size
                        continue;
                    }
                    else
                    {
                        _previousBuffer = data.Slice(index).ToArray();
                        break;
                    }
                }
                // We can expect the file header bytes before the message sizes.
                else if ((data.Length - index) >= 15 && 
                    data.Slice(index, 11).SequenceEqual([(byte)'U', (byte)'3', (byte)'r', (byte)'a', (byte)'w', (byte)'[', (byte)'$', (byte)'U', (byte)'#', (byte)'l']))
                {
                    // If we got the entire header [U][3][r][a][w][[][$][U][#][l][X][X][X][X] we can skip it entirely
                    // It's not worth reading the length as it might still be zeroed
                    index += 15;
                    continue;
                }
                else
                {
                    // Consume 1 byte at a time until we get the MESSAGE_SIZES command
                    index += 1;
                    continue;
                }
            }
            else if (_payloadSizes.TryGetValue(command, out int payloadSize) && payloadSize > 0)
            {
                // Make sure we have enough data to read a full payload
                int remainingLen = data.Length - index;
                if (remainingLen < payloadSize + 1) // + 1 for the command byte we're still sitting on
                {
                    // If remaining length is not long enough for full payload, save the remaining
                    // data until we receive more data. The data has been split up.
                    _previousBuffer = data.Slice(index).ToArray();
                    break;
                }

                Span<byte> payload = [..data.Slice(index, payloadSize + 1)];
                try
                {
                    EmitRawCommand(command, payload);
                    ProcessCommand(command, payload);
                }
                catch (Exception)
                {
                    // Only throw the error if we're not suppressing the errors
                    if (!_settings.SuppressErrors)
                    {
                        throw;
                    }
                }

                index += payloadSize + 1;
            }
            else
            {
                // Unexpected data - just keep going until we get a command we can parse
                index += 1;
                continue;
            }
        }
    }

    private void EmitRawCommand(Command command, in ReadOnlySpan<byte> payload)
    {
        // Forward the raw buffer onwards
        OnRaw?.Invoke(this, new SlpStreamRawEventArgs() { Command = command, Payload = payload.ToArray() });
    }

    private void ProcessCommand(Command command, in Span<byte> payload)
    {
        if (payload.Length > 0)
        {
            Debug.Assert(_payloadSizes![command] == payload.Length - 1, "Unexpected payload size");

            if (command == Command.SPLIT_MESSAGE)
            {
                ProcessSplitMessageCommand(payload);
            }
            else
            {
                EventPayload? parsedPayload = SlpFile.ParseMessage(command, payload);

                if (parsedPayload is null)
                {
                    Debug.Fail("Failed to parse payload?");
                    return;
                }

                if (command == Command.GAME_END && _settings.Mode == SlpStreamModes.MANUAL)
                {
                    // Stop parsing data until we manually restart the stream
                    _gameEnded = true;
                }

                OnCommand?.Invoke(this, new SlpStreamCommandEventArgs() { Command = command, Payload = parsedPayload });
            }
        }
        else
        {
            throw new Exception("0-length command?");
        }
    }

    private void ProcessMessageSizesCommand(in Span<byte> payload)
    {
        _payloadSizes = [];

        BufferReader x = new BufferReader(payload);

        // 0x1: Payload size - The size in bytes of the payload for this event, including this byte (i.e. 3n+1, where n is the number of commands to follow)
        byte messageSizePayloadLen = x.ReadUInt8(0x1) ?? throw new Exception("Failed to parse payload length from stream");
        _payloadSizes[Command.MESSAGE_SIZES] = messageSizePayloadLen;
        Debug.Assert(messageSizePayloadLen <= x.Length, "Stream does not have enough space to process this command?");
        
        for (int i = 0x2; i < messageSizePayloadLen - 1; i += 3)
        {
            Command command = x.ReadUInt8(i).EnumCast<Command>() ?? throw new Exception("Failed to parse command from stream");
            ushort payloadSize = x.ReadUInt16(i + 1) ?? throw new Exception("Failed to parse payload length from stream");

            _payloadSizes[command] = payloadSize;
        }

        // Emit the raw command event
        EmitRawCommand(Command.MESSAGE_SIZES, payload);

        // Emit the typed command event
        OnCommand?.Invoke(this, new SlpStreamCommandEventArgs() { Command = Command.MESSAGE_SIZES, Payload = _payloadSizes });
    }

    private void ProcessSplitMessageCommand(in Span<byte> payload)
    {
        BufferReader x = new BufferReader(payload);
        Debug.Assert(x.ReadUInt8(0).EnumCast<Command>() == Command.SPLIT_MESSAGE, "Called wrong handler?");

        Command actualCommand = x.ReadUInt8(0x203).EnumCast<Command>() ?? throw new Exception("Failed to parse internal command from split message");
        int actualSize = x.ReadUInt16(0x201) ?? throw new Exception("Failed to parse internal payload length from split message");
        bool isLastMessage = x.ReadBool(0x204) ?? throw new Exception("Failed to determine isLastMessage from split message");

        if (_splitBuffer.Length == 0)
        {
            _splitBuffer = [(byte)actualCommand, ..payload.Slice(0x1, actualSize)];
        }
        else
        {
            Debug.Assert(Unsafe.As<byte, Command>(ref _splitBuffer[0]) == actualCommand, "Mismatched split messages?");
            _splitBuffer = [.._splitBuffer, ..payload.Slice(0x1, actualSize)];
        }

        if (isLastMessage)
        {
            ProcessCommand(Unsafe.As<byte, Command>(ref _splitBuffer[0]), _splitBuffer);
            _splitBuffer = [];
        }
    }
}
