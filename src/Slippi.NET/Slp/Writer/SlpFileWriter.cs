using Slippi.NET.Slp.EventStream;
using Slippi.NET.Slp.EventStream.Types;
using Slippi.NET.Slp.Reader.File;
using Slippi.NET.Types;

namespace Slippi.NET.Slp.Writer;

/// <summary>
/// This class wraps a writable <see cref="SlpEventStream"/> and provides the ability
/// to write multiple files (as opposed to using a single <see cref="SlpFileStream"/>), toggle if files should be written, 
/// and emits events when files are created and completed.
/// </summary>
public class SlpFileWriter : SlpEventStream, IDisposable
{
    private SlpFileStream? _currentFile = null;
    private SlpFileWriterSettings _settings;

    public SlpFileWriter(SlpFileWriterSettings? settings = null) : base(settings) 
    {
        // slightly lazy
        _settings = settings ?? new SlpFileWriterSettings();

        OnRaw += OnRawData;
    }

    /// <summary>
    /// Emitted when a new game is detected in the stream and a file has been opened for writing. <br/>
    /// EventArgs: The absolute filepath of the .slp file.
    /// </summary>
    public event EventHandler<string>? OnNewFile;

    /// <summary>
    /// Emitted when a game has been completed and a file has been written and closed. <br/>
    /// EventArgs: The absolute filepath of the .slp file.
    /// </summary>
    public event EventHandler<string>? OnFileComplete;

    public string? GetCurrentFilename()
    {
        if (_currentFile is not null)
        {
            return Path.GetFullPath(_currentFile.GetFilePath());
        }

        return null;
    }

    public void EndCurrentFile()
    {
        HandleEndGame();
    }

    public void UpdateSettings(SlpFileWriterSettings settings)
    {
        _settings = settings;
    }

    private void WritePayload(in ReadOnlySpan<byte> payload)
    {
        if (_currentFile is not null)
        {
            _currentFile.Write(payload);
        }
    }

    private void OnRawData(object? sender, SlpStreamRawEventArgs args)
    {
        switch (args.Command)
        {
            case Command.MESSAGE_SIZES:
                {
                    // Create the new game first before writing the payload
                    HandleNewGame();
                    WritePayload(args.Payload);

                    break;
                }
            case Command.GAME_END:
                {
                    // Write payload first before ending the game
                    WritePayload(args.Payload);
                    HandleEndGame();

                    break;
                }
            default:
                {
                    WritePayload(args.Payload);
                    break;
                }
        }
    }

    private void HandleNewGame()
    {
        // Only create a new file if we're outputting files
        if (_settings.OutputFiles)
        {
            string filePath = _settings.MakeNewFileName(_settings.FolderPath, DateTime.Now);
            _currentFile = new SlpFileStream(filePath, this);

            OnNewFile?.Invoke(this, Path.GetFullPath(filePath));
        }
    }

    private void HandleEndGame()
    {
        // End the stream
        if (_currentFile is not null)
        {
            // Set the console nickname
            _currentFile.UpdateMetadata(_settings.ConsoleNickname, _settings.PlayerNameOverrides);
            _currentFile.Close();

            OnFileComplete?.Invoke(this, Path.GetFullPath(_currentFile.GetFilePath()));

            // Clear current file
            _currentFile = null;
        }
    }

    public void Dispose()
    {
        _currentFile?.Dispose();
    }
}
