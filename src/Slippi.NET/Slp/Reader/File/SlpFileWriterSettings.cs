using Slippi.NET.Slp.EventStream.Types;
using Slippi.NET.Types;

namespace Slippi.NET.Slp.Reader.File;

public record class SlpFileWriterSettings : SlpStreamSettings
{
    /// <summary>
    /// [Optional] If the stream should write files.
    /// </summary>
    public bool OutputFiles { get; set; } = true;

    /// <summary>
    /// [Optional] The folder path at which files should be written to.
    /// </summary>
    public string FolderPath { get; set; } = Directory.GetCurrentDirectory();

    /// <summary>
    /// [Optional] Nickname to give to the console.
    /// </summary>
    public string ConsoleNickname { get; set; } = "unknown";

    /// <summary>
    /// [Optional] Player name info overrides. Useful for updating replays
    /// gathered from console.
    /// </summary>
    public Dictionary<int, PlayerNameInfo>? PlayerNameOverrides { get; set; } = null;

    /// <summary>
    /// [Optional] Delegate to compute file names given a folder name and date when creating new files.
    /// </summary>
    public Func<string, DateTime, string> MakeNewFileName { get; set; } =
        static (string folder, DateTime date) => Path.Join(folder, $"Game_{date.ToString("yyyyMMdd")}T{date.ToString("HHmmss")}.slp");
}
