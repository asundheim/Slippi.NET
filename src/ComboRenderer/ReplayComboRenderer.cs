using ComboInterpreter;
using Slippi.NET.Console;
using Slippi.NET.Console.Types;
using Slippi.NET.Types;
using System.Windows;

namespace ComboRenderer;

internal class ReplayComboRenderer : BaseComboRenderer
{
    private Window _window;
    private DolphinLauncher? _dolphinLauncher;
    private FoxComboInterpreter? _comboBot;

    private string? _replayPath = null;
    private int? _startFrame = null;

    private IList<QueueItem>? _replays = null;

    public ReplayComboRenderer(Window window, string replayPath, int startFrame = (int)Frames.FIRST) : base()
    { 
        _window = window;
        _replayPath = replayPath;
        _startFrame = startFrame;
    }

    public ReplayComboRenderer(Window window, IList<QueueItem> replays) : base()
    {
        _window = window;
        _replays = replays;
    }

    public override void Begin()
    {
        _dolphinLauncher = new DolphinLauncher(@"C:\Users\ander\Downloads\meleeout.iso");

        _dolphinLauncher.OnPlaybackStartFrameAndFilePath += (object? sender, PlaybackFilePathAndStartFrameEventArgs args) =>
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            _cancellationToken = _cts.Token;

            _comboBot = new FoxComboInterpreter(args.FilePath, args.StartFrame, "george seinfeld", "ders", "D#345", "D#10");

            InvokeNewGame(_comboBot);
        };

        _dolphinLauncher.OnReplayedFrame += (object? sender, int frame) =>
        {
            _comboBot?.ProcessFrame(frame);
        };

        _dolphinLauncher.OnPlaybackComplete += (_, _) =>
        {
            _cts?.Cancel();
        };

        _dolphinLauncher.OnDolphinClosed += (_, _) =>
        {
            _cts?.Cancel();
            _window.Dispatcher.BeginInvoke(() => _window.Close());
        };

        DolphinLaunchArgs launchArgs;
        if (_replayPath is not null && _startFrame is not null)
        {
            launchArgs = new DolphinLaunchArgs()
            {
                Replay = _replayPath,
                StartFrame = _startFrame.Value,
                EndFrame = int.MaxValue
            };
        }
        else if (_replays is not null)
        {
            launchArgs = new DolphinLaunchArgs()
            {
                Mode = DolphinLaunchModes.Queue,
                Queue = _replays,
            };
        }
        else 
        {
            throw new Exception();
        }

        _dolphinLauncher.LaunchDolphin(launchArgs);
    }

    public override void Dispose()
    {
        _dolphinLauncher?.Dispose();
    }
}
