using DolphinConnectionTestApp;
using Slippi.NET.Console;
using Slippi.NET.Console.Types;
using System.Diagnostics;

namespace DolphinTestApp;

public class DolphinLaunchTestApp : IDisposable
{
    private readonly string _replayPath;
    private readonly string? _dolphinPath;
    private string? _meleeIsoPath;
    private readonly CancellationTokenSource _cts = new CancellationTokenSource();
    private DolphinLauncher? _launcher = null;
    public DolphinLaunchTestApp(string replayPath, string? dolphinPath = null, string? meleeIsoPath = null)
    {
        _replayPath = replayPath;
        _dolphinPath = dolphinPath;
        _meleeIsoPath = meleeIsoPath;
    }

    public void LaunchAndWait()
    {
        _meleeIsoPath ??= @"C:\isos\meleeout.iso";
        _launcher = new DolphinLauncher(_meleeIsoPath);
        _launcher.OnPlaybackComplete += OnPlaybackComplete;

        _ = Task.Run(() =>
        {
            _launcher.LaunchDolphin(new DolphinLaunchArgs() { Replay = _replayPath, EndFrame = 100 });
        });

        Console.ReadLine();
    }

    private void OnPlaybackComplete(object? sender, EventArgs args)
    {
        Console.WriteLine("Playback complete");
        _cts.Cancel();

        Dispose();
        Environment.Exit(0);
    }

    public void Dispose()
    {
        _launcher?.Dispose();
        _cts.Dispose();
    }
}
