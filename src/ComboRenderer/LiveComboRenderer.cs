using ComboInterpreter;
using OBSWebsocketDotNet;
using Slippi.NET.Console;
using Slippi.NET.Console.Types;
using Slippi.NET.Slp.Reader.File;
using Slippi.NET.Slp.Writer;

namespace ComboRenderer;

internal class LiveComboRenderer : BaseComboRenderer
{
    private DolphinConnection? _connection;
    private SlpFileWriter? _fileWriter;

    public LiveComboRenderer() : base()
    {
    }

    public override void Begin(OBSWebsocket? obs = null)
    {
        // live
        _connection = new DolphinConnection();
        _fileWriter = new SlpFileWriter(new SlpFileWriterSettings() 
        { 
            FolderPath = System.IO.Path.GetTempPath() 
        });

        _connection.OnStatusChange += (object? sender, ConnectionStatus status) => InvokeOnStatusChange(status);

        _connection.OnData += (object? sender, byte[] data) =>
        {
            _fileWriter.Write(data);
        };

        _fileWriter.OnNewFile += (object? sender, string path) =>
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            _ = Task.Run(async () =>
            {
                await Task.Delay(1000);

                _cancellationToken = _cts.Token;
                var comboBot = new FoxComboInterpreter(path, [..SettingsManager.Instance.Settings.ConnectCodes, ..SettingsManager.Instance.Settings.DisplayNames]);

                InvokeNewGame(comboBot);
            });
        };

        _fileWriter.OnFileComplete += (_, _) =>
        {
            InvokeGameEnd();
            _cts.Cancel();
        };

        _ = Task.Run(async () =>
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                try
                {
                    _connection.Connect("127.0.0.1", (int)Ports.Default, true, 10_000);
                    break;
                }
                catch
                {
                    // wait 5s between attempts
                    await Task.Delay(5_000);
                }
            }
        });
    }

    public override void Dispose()
    {
        base.Dispose();

        _connection?.Dispose();
        _fileWriter?.Dispose();
    }
}
