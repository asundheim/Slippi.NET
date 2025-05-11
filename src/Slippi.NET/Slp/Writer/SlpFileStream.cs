using Slippi.NET.Melee.Types;
using Slippi.NET.Slp.EventStream;
using Slippi.NET.Slp.EventStream.Types;
using Slippi.NET.Types;
using System.Buffers.Binary;
using System.Globalization;
using System.Text;

namespace Slippi.NET.Slp.Writer;

/// <summary>
/// This class wraps a writable .slp file stream. It handles writing the binary
/// header and footer, as well as the overwriting of the raw data length.
/// </summary>
public class SlpFileStream : IDisposable
{
    private readonly string _filePath;
    private readonly Metadata _metadata;
    private readonly SlpEventStream _eventStream;
    private readonly bool _usesExternalStream;

    private FileStream? _fileStream = null;
    private uint _rawDataLength = 0;

    public SlpFileStream(string filePath, SlpEventStream? eventStream = null)
    {
        _filePath = filePath;
        _metadata = new Metadata()
        {
            ConsoleNick = "unknown",
            StartAt = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
            LastFrame = (int)Frames.FIRST - 1,
            Players = []
        };

        _usesExternalStream = eventStream is not null;
        // Create a new SlpStream if one wasn't already provided
        // This SLP stream represents a single game not multiple, so use manual mode
        _eventStream = eventStream ?? new SlpEventStream(new SlpStreamSettings() { Mode = SlpStreamModes.MANUAL });

        _eventStream.OnCommand += OnNewCommand;
        InitializeNewGame();
    }

    public string GetFilePath() => _filePath;

    // TODO this can take more arguments but current usage in slippi-js just sets console nick
    public void UpdateMetadata(string? consoleNick = null)
    {
        if (consoleNick is not null)
        {
            _metadata.ConsoleNick = consoleNick;
        }
    }

    public void Write(in ReadOnlySpan<byte> chunk)
    {
        // Write it to the file
        if (_fileStream is not null)
        {
            _fileStream.Write(chunk);
        }

        // Parse the data manually if it's an internal stream (otherwise it's assumed the owner is writing to it)
        if (!_usesExternalStream)
        {
            _eventStream.Write(chunk);
        }

        // Keep track of the bytes we've written
        _rawDataLength += (uint)chunk.Length;
    }

    private void InitializeNewGame()
    {
        // 0 buffer to ensure data is immediately flushed to the file, in case others are reading it
        _fileStream = new FileStream(_filePath, FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite, bufferSize: 0, useAsync: true);

        ReadOnlySpan<byte> header = 
        [
            ..Encoding.UTF8.GetBytes("{U"),
            0x3,
            ..Encoding.UTF8.GetBytes("raw[$U#l"),
            0x0, 0x0, 0x0, 0x0
        ];

        _fileStream.Write(header);
    }

    private void OnNewCommand(object? sender, SlpStreamCommandEventArgs args)
    {
        switch (args.Command)
        {
            case Command.GAME_START:
                HandleGameStart((GameStartPayload)args.Payload);
                break;
            case Command.POST_FRAME_UPDATE:
                HandlePostFrameUpdate((PostFrameUpdatePayload)args.Payload);
                break;
            default:
                break;
        }
    }

    private void HandleGameStart(GameStartPayload payload)
    {
        foreach (var player in payload.GameStart.Players)
        {
            if (player.Type == 3)
            {
                continue;
            }

            _metadata.Players[player.PlayerIndex] = new PlayerMetadata()
            {
                Names = new PlayerNameInfo()
                {
                    Netplay = player.DisplayName,
                    Code = player.ConnectCode
                },
                CharacterUsage = []
            };
        }
    }

    private void HandlePostFrameUpdate(PostFrameUpdatePayload payload)
    {
        if (payload.PostFrameUpdate.IsFollower == true)
        {
            // No need to do this for follower
            return;
        }

        // Update frame index
        _metadata.LastFrame = payload.PostFrameUpdate.Frame;

        // Update character usage
        PlayerMetadata prevPlayer = _metadata.Players[payload.PostFrameUpdate.PlayerIndex ?? throw new Exception("No player for frame?")];
        Character character = (Character)payload.PostFrameUpdate.InternalCharacterId!;
        int curCharFrames = prevPlayer.CharacterUsage.TryGetValue(character, out var existing) ? existing : 0;

        prevPlayer.CharacterUsage[character] = curCharFrames + 1;
    }

    private void WriteFooter()
    {
        // assuming we don't have more than 8MB footer? this is probably a very wasteful alloc
        byte[] buffer = new byte[8092];
        MemoryStream footer = new MemoryStream(buffer, writable: true);
        footer.Seek(0, SeekOrigin.Begin);

        footer.Write(
            [
                (byte)'U',
                0x8,
                ..Encoding.UTF8.GetBytes("metadata{")
            ]
        );

        // Write game start time
        footer.Write(
            [
                (byte)'U',
                0x7,
                ..Encoding.UTF8.GetBytes("startAtSU"),
                (byte)(_metadata.StartAt!.Length),
                ..Encoding.UTF8.GetBytes(_metadata.StartAt!)
            ]
        );

        // Write last frame index
        // TODO: Get last frame (5 years ago)
        footer.Write(
            [
                (byte)'U',
                0x9,
                ..Encoding.UTF8.GetBytes("lastFramel"),
                ..GetInt32BigEndian(_metadata.LastFrame ?? (int)Frames.FIRST - 1)
            ]
        );

        // write the Console Nickname
        footer.Write(
            [
                (byte)'U',
                (byte)11,
                ..Encoding.UTF8.GetBytes("consoleNickSU"),
                (byte)(_metadata.ConsoleNick!.Length),
                ..Encoding.UTF8.GetBytes(_metadata.ConsoleNick!)
            ]
        );

        // Start writing player specific data
        footer.Write(
            [
                (byte)'U',
                0x7,
                ..Encoding.UTF8.GetBytes("players{")
            ]
        );

        for (int i = 0; i < _metadata.Players.Values.Count; i++)
        {
            // Start player obj with index being the player index
            var player = _metadata.Players[i];
            string playerIndexString = $"{i}";
            footer.Write(
                [
                    (byte)'U',
                    (byte)playerIndexString.Length,
                    ..Encoding.UTF8.GetBytes(playerIndexString + "{")
                ]
            );

            // Start characters key for this player
            footer.Write(
                [
                    (byte)'U',
                    (byte)10,
                    ..Encoding.UTF8.GetBytes("characters{")
                ]
            );

            // Write character usage
            foreach (var kvp in player.CharacterUsage)
            {
                var character = kvp.Key;
                var usage = kvp.Value;

                // Write this character
                string usageIndexString = $"{(int)character}";
                footer.Write(
                    [
                        (byte)'U',
                        (byte)usageIndexString.Length,
                        ..Encoding.UTF8.GetBytes($"{usageIndexString}l"),
                        ..GetInt32BigEndian(usage)
                    ]
                );
            }

            // Close characters
            footer.Write([(byte)'}']);

            // Start names key for this player
            footer.Write(
                [
                    (byte)'U',
                    0x5,
                    ..Encoding.UTF8.GetBytes("names{")
                ]
            );

            // Write display name
            footer.Write(
                [
                    (byte)'U',
                    0x7,
                    ..Encoding.UTF8.GetBytes("netplaySU"),
                    (byte)(player.Names?.Netplay?.Length ?? 0),
                    ..Encoding.UTF8.GetBytes(player.Names?.Netplay ?? string.Empty)
                ]
            );

            // Write connect code
            footer.Write(
                [
                    (byte)'U',
                    0x4,
                    ..Encoding.UTF8.GetBytes("codeSU"),
                    (byte)(player.Names?.Code?.Length ?? 0),
                    ..Encoding.UTF8.GetBytes(player.Names?.Code ?? string.Empty)
                ]
            );

            // Close names and player
            footer.Write(Encoding.UTF8.GetBytes("}}"));
        }

        // Close players
        footer.Write(Encoding.UTF8.GetBytes("}"));

        // Write played on
        footer.Write(
            [
                (byte)'U',
                0x8,
                ..Encoding.UTF8.GetBytes("playedOnSU"),
                0x7,
                ..Encoding.UTF8.GetBytes("network")
            ]
        );

        // Close metadata and file
        footer.Write(Encoding.UTF8.GetBytes("}}"));

        // Write to stream
        long length = footer.Position;
        Span<byte> footerBytes = footer.ToArray().AsSpan().Slice(0, (int)length);

        _fileStream!.Write(footerBytes);
    }

    private static ReadOnlySpan<byte> GetInt32BigEndian(int value)
    {
        Span<byte> bytes = new byte[sizeof(uint)];
        BinaryPrimitives.WriteInt32BigEndian(bytes, value);

        return bytes;
    }

    private static ReadOnlySpan<byte> GetUInt32BigEndian(uint value)
    {
        Span<byte> bytes = new byte[sizeof(uint)];
        BinaryPrimitives.WriteUInt32BigEndian(bytes, value);

        return bytes;
    }

    /// <summary>
    /// Explicitly close the stream.
    /// </summary>
    /// <remarks>
    /// This is the same as calling <see cref="Dispose"/>.
    /// </remarks>
    public void Close()
    {
        if (_fileStream is not null)
        {
            // Write footer
            WriteFooter();

            // Update file with bytes written
            _fileStream.Flush();
            _fileStream.Seek(11, SeekOrigin.Begin);

            Span<byte> rawDataLengthBuffer = stackalloc byte[sizeof(uint)];
            BinaryPrimitives.WriteUInt32BigEndian(rawDataLengthBuffer, _rawDataLength);
            _fileStream.Write(rawDataLengthBuffer);
            _fileStream.Flush();

            _fileStream.Dispose();
        }

        _eventStream.OnCommand -= OnNewCommand;
    }

    public void Dispose()
    {
        Close();
    }
}
