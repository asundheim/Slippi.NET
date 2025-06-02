using Newtonsoft.Json;
using Slippi.NET.Console.Types;
using Slippi.NET.Types;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Slippi.NET.Console;

public record class PlaybackFilePathAndStartFrameEventArgs
{
    public required string FilePath { get; init; }
    public required int StartFrame { get; init; }
}

public partial class DolphinLauncher : IDisposable
{
    private readonly string _dolphinPath;
    private readonly string _meleePath;
    private readonly CancellationTokenSource _cts = new CancellationTokenSource();
    private readonly BlockingCollection<int> _frames = new BlockingCollection<int>();

    private bool _gotGameEnd = false;
    private bool _gotPlaybackEndFrame = false;
    private bool _gotPlaybackStartFrame = false;
    private bool _gotFilePath = false;

    private int _gameEndFrame = int.MaxValue;
    private int _playbackStartFrame = (int)Frames.FIRST;
    private int _playbackEndFrame = int.MaxValue;
    private string _filePath = string.Empty;

    private IList<QueueItem>? _queue = null;
    private int? _queueEnd = null;

    private int _gameCounter = 0;

    private Process? _launchedDolphin = null;

    [GeneratedRegex(@"\[CURRENT_FRAME\] (-?\d+)", RegexOptions.Compiled)]
    private static partial Regex _frameRegex();

    [GeneratedRegex(@"\[GAME_END_FRAME\] (-?\d+)", RegexOptions.Compiled)]
    private static partial Regex _gameEndRegex();

    [GeneratedRegex(@"\[PLAYBACK_END_FRAME\] (-?\d+)", RegexOptions.Compiled)]
    private static partial Regex _playbackEndRegex();

    [GeneratedRegex(@"\[PLAYBACK_START_FRAME\] (-?\d+)", RegexOptions.Compiled)]
    private static partial Regex _playbackStartRegex();

    [GeneratedRegex(@"\[FILE_PATH\] (.+)", RegexOptions.Compiled)]
    private static partial Regex _filePathRegex();
    /// <remarks>
    /// With the dolphin path unspecified (as opposed to <see cref="DolphinLauncher(string, string)"/>)
    /// this constructor will attempt to use the default install path of
    /// <code>
    /// "%APPDATA%\Slippi Launcher\playback\Slippi Dolphin.exe"
    /// </code>
    /// </remarks>
    public DolphinLauncher(string meleeIsoPath) : this(meleeIsoPath, TryFindDolphinReplayExe()) { }

    /// <param name="meleeIsoPath">The fully qualified path a SSBM .iso file.</param>
    /// <param name="dolphinPath">The fully qualified path to <c>Slippi Dolphin.exe</c>.</param>
    public DolphinLauncher(string meleeIsoPath, string dolphinPath)
    {
        if (string.IsNullOrEmpty(dolphinPath) || !File.Exists(dolphinPath))
        {
            throw new ArgumentException("dolphinPath must be a valid, fully-qualified path to Slippi Dolphin.exe");
        }

        _dolphinPath = dolphinPath;

        if (string.IsNullOrEmpty(meleeIsoPath) || !File.Exists(meleeIsoPath))
        {
            throw new ArgumentException("meleeIsoPath must be a valid, fully-qualified path to a melee .iso file");
        }

        _meleePath = meleeIsoPath;
    }

    /// <summary>
    /// Emitted when dolphin has sent both the PLAYBACK_START_FRAME frame number and the FILE_PATH filepath.
    /// </summary>
    public event EventHandler<PlaybackFilePathAndStartFrameEventArgs>? OnPlaybackStartFrameAndFilePath;

    /// <summary>
    /// Emitted every frame during playback when Dolphin indicates that the given frame number has been played back.
    /// </summary>
    public event EventHandler<int>? OnReplayedFrame;

    /// <summary>
    /// Emitted when playback of the replay completes.
    /// </summary>
    public event EventHandler? OnPlaybackComplete;

    /// <summary>
    /// Emitted when the Dolphin window is closed.
    /// </summary>
    public event EventHandler? OnDolphinClosed;

    public void LaunchDolphin(DolphinLaunchArgs args)
    {
        if (args.Mode == DolphinLaunchModes.Queue)
        {
            _queue = args.Queue;
            _queueEnd = args.Queue[^1].EndFrame;
        }

        string tempLaunchFile = Path.Join(Path.GetTempPath(), "tempLaunch.json");
        File.WriteAllText(tempLaunchFile, JsonConvert.SerializeObject(args, new JsonSerializerSettings() 
        { 
            DefaultValueHandling = DefaultValueHandling.Ignore, 
            Formatting = Formatting.Indented
        }));

        ProcessStartInfo processStart = new ProcessStartInfo()
        {
            FileName = _dolphinPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            Arguments = $"-i \"{tempLaunchFile}\" -e \"{_meleePath}\" --cout"
        };

        _launchedDolphin = new Process();
        _launchedDolphin.EnableRaisingEvents = true;
        _launchedDolphin.StartInfo = processStart;
        
        _launchedDolphin.OutputDataReceived += OnDolphinStdOut;
        _launchedDolphin.ErrorDataReceived += OnDolphinStdErr;
        _launchedDolphin.Exited += OnDolphinExit;

        _ = Task.Run(() => ProcessReplayedFrames(_cts.Token));

        _launchedDolphin.Start();
        _launchedDolphin.BeginOutputReadLine();
        _launchedDolphin.BeginErrorReadLine();
    }

    private void OnDolphinStdErr(object? sender, DataReceivedEventArgs args)
    {
        System.Console.WriteLine(args.Data);
    }

    private void OnDolphinStdOut(object? sender, DataReceivedEventArgs args)
    {
        if (args.Data is not null)
        {
            if (!_gotGameEnd)
            {
                Match mGameEnd = _gameEndRegex().Match(args.Data);
                if (mGameEnd.Success)
                {
                    _gotGameEnd = true;
                    _gameEndFrame = int.Parse(mGameEnd.Groups[1].Value);
                    System.Console.WriteLine($"Game End Frame: {_gameEndFrame}");

                    return;
                }
            }

            if (!_gotPlaybackEndFrame)
            {
                Match playbackEnd = _playbackEndRegex().Match(args.Data);
                if (playbackEnd.Success)
                {
                    _gotPlaybackEndFrame = true;
                    System.Console.WriteLine($"Playback End Frame: {playbackEnd.Groups[1].Value}");
                    if (int.TryParse(playbackEnd.Groups[1].Value, out int playbackEndFrame))
                    {
                        _playbackEndFrame = playbackEndFrame;
                    }

                    return;
                }
            }

            if (!_gotPlaybackStartFrame)
            {
                Match playbackStart = _playbackStartRegex().Match(args.Data);
                if (playbackStart.Success)
                {
                    _gotPlaybackStartFrame = true;
                    _playbackStartFrame = int.Parse(playbackStart.Groups[1].Value);
                    System.Console.WriteLine($"Playback Start Frame: {_playbackStartFrame}");

                    if (_gotFilePath)
                    {
                        OnPlaybackStartFrameAndFilePath?.Invoke(this, new PlaybackFilePathAndStartFrameEventArgs()
                        {
                            FilePath = _filePath,
                            StartFrame = _playbackStartFrame
                        });

                        _gameCounter++;
                    }
                    
                    return;
                }
            }

            if (!_gotFilePath)
            {
                Match filePath = _filePathRegex().Match(args.Data);
                if (filePath.Success)
                {
                    _gotFilePath = true;
                    _filePath = filePath.Groups[1].Value;
                    System.Console.WriteLine($"File path: {_filePath}");

                    if (_gotPlaybackStartFrame)
                    {
                        OnPlaybackStartFrameAndFilePath?.Invoke(this, new PlaybackFilePathAndStartFrameEventArgs()
                        {
                            FilePath = _filePath,
                            StartFrame = _playbackStartFrame
                        });

                        _gameCounter++;
                    }

                    return;
                }
            }

            Match m = _frameRegex().Match(args.Data);
            if (m.Success)
            {
                int frameNum = int.Parse(m.Groups[1].Value);
                if (frameNum == _playbackEndFrame || frameNum == _gameEndFrame)
                {
                    _gotFilePath = false;
                    _gotGameEnd = false;
                    _gotPlaybackEndFrame = false;
                    _gotPlaybackStartFrame = false;
                }
                
                _frames.Add(frameNum);
            }
        }
    }

    private void OnDolphinExit(object? sender, EventArgs args)
    {
        _cts.Cancel();
        OnDolphinClosed?.Invoke(this, args);
    }

    private void ProcessReplayedFrames(CancellationToken cancellation)
    {
        while (!cancellation.IsCancellationRequested)
        {
            int frame = _frames.Take(cancellation);
            OnReplayedFrame?.Invoke(this, frame);

            if (frame == _playbackEndFrame || frame == _gameEndFrame)
            {
                OnPlaybackComplete?.Invoke(this, EventArgs.Empty);

                if (_queueEnd is null || (_gameCounter == _queue?.Count && _queueEnd == frame))
                {
                    _launchedDolphin?.Kill();
                }
            }
        }
    }

    private static string TryFindDolphinReplayExe()
    {
        string defaultPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"\Slippi Launcher\playback", "Slippi Dolphin.exe");
        if (File.Exists(defaultPath))
        {
            return defaultPath;
        }

        return string.Empty;
    }

    public void Dispose()
    {
        if (_launchedDolphin is not null)
        {
            _launchedDolphin.OutputDataReceived -= OnDolphinStdOut;
            _launchedDolphin.ErrorDataReceived -= OnDolphinStdErr;
            _launchedDolphin.Kill();
            _launchedDolphin.Dispose();
        }
        
        _cts.Dispose();
    }
}
