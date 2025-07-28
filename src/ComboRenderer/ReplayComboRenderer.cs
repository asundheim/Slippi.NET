using ComboInterpreter;
using OBSWebsocketDotNet;
using Slippi.NET.Console;
using Slippi.NET.Console.Types;
using Slippi.NET.Types;
using System.IO;
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

    public override void Begin(OBSWebsocket? obs = null)
    {
        _dolphinLauncher = new DolphinLauncher(@"C:\Users\ander\Downloads\meleeout.iso");

        _dolphinLauncher.OnPlaybackStartFrameAndFilePath += (object? sender, PlaybackFilePathAndStartFrameEventArgs args) =>
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            _cancellationToken = _cts.Token;

            _startFrame = args.StartFrame;
            obs?.SetProfileParameter("Output", "FilenameFormatting", Path.GetFileNameWithoutExtension(args.FilePath));

            if (_comboBot is not null)
            {
                _comboBot.OnDI -= HandleDI;
            }

            _comboBot = new FoxComboInterpreter(args.FilePath, args.StartFrame, [..SettingsManager.Instance.Settings.ConnectCodes, ..SettingsManager.Instance.Settings.DisplayNames]);
            _comboBot.OnDI += HandleDI;

            InvokeNewGame(_comboBot);
        };

        _dolphinLauncher.OnReplayedFrame += (object? sender, int frame) =>
        {
            if (_startFrame + 2 /* avoid recording the white frames if it's seeking */ == frame)
            {
                obs?.StartRecord();
            }

            _comboBot?.ProcessFrame(frame);
        };

        _dolphinLauncher.OnPlaybackComplete += (_, _) =>
        {
            _cts?.Cancel();
            obs?.StopRecord();
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
            throw new Exception("?");
        }

        _dolphinLauncher.LaunchDolphin(launchArgs);
    }

    private void HandleDI(object? sender, DIEventArgs e)
    {
        InvokeDI(e);
    }

    public override void Dispose()
    {
        base.Dispose();

        _dolphinLauncher?.Dispose();
    }
}
