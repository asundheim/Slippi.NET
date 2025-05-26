using ComboInterpreter;
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

    public override void Begin()
    {
        // live
        _connection = new DolphinConnection();
        _fileWriter = new SlpFileWriter(new SlpFileWriterSettings() { FolderPath = System.IO.Path.GetTempPath() });

        _connection.OnHandshake += (object? sender, ConnectionDetails args) =>
        {
            Console.WriteLine("Connected");
        };

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
                var comboBot = new FoxComboInterpreter(path, "george seinfeld", "ders", "D#345", "D#10");

                InvokeNewGame(comboBot);
            });
        };

        _fileWriter.OnFileComplete += (_, _) =>
        {
            _cts.Cancel();
        };

        _connection.Connect("127.0.0.1", (int)Ports.Default, true, 30_000);
    }

    public override void Dispose()
    {
        _connection?.Dispose();
        _fileWriter?.Dispose();
    }
}
