using Slippi.NET.Console;
using Slippi.NET.Console.Types;
using Slippi.NET.Slp.EventStream.Types;
using Slippi.NET.Slp.Reader.File;
using Slippi.NET.Slp.Writer;

namespace DolphinConnectionTestApp;

/// <summary>
/// Test app / example for <see cref="DolphinConnection"/>.
/// </summary>
/// <remarks>
/// This doesn't actually launch Dolphin with a replay (yet), it is assumed Dolphin is already running.
/// </remarks>
public class DolphinConnectionTestApp
{
    private readonly SlpFileWriter _fileWriter;

    public DolphinConnectionTestApp()
    {
        _fileWriter = new SlpFileWriter(new SlpFileWriterSettings()
        {
            FolderPath = Path.GetTempPath(),
            Mode = SlpStreamModes.AUTO,
            ConsoleNickname = "test-dolphin",
            OutputFiles = true
        });

        _fileWriter.OnFileComplete += OnFileComplete;
        _fileWriter.OnCommand += OnStreamCommand;
    }

    private void OnFileComplete(object? sender, string e)
    {
        Console.WriteLine("New replay available at: ");
        Console.WriteLine(e);
    }

    private void OnStreamCommand(object? sender, SlpStreamCommandEventArgs e)
    {
        Console.WriteLine($"Command: {e.Command.ToString()}");
        Console.WriteLine($"Event type: {e.Payload.GetType().Name}");
    }

    public void ConnectAndWait()
    {
        DolphinConnection connection = new DolphinConnection();
        connection.OnMessage += Connection_OnMessage;
        connection.OnHandshake += Connection_OnHandshake;
        connection.OnData += Connection_OnData;
        connection.OnNewMenuEvent += Connection_OnMenuEvent;

        connection.Connect("127.0.0.1", (int)Ports.Default, isRealtime: true, timeout: 10_000);
        Console.ReadLine();

        connection.HandleDisconnect();
        connection.Dispose();
        Console.WriteLine("Disconnected.");

        _fileWriter.Dispose();
    }

    private void Connection_OnMenuEvent(object? sender, MenuEvent e)
    {
        Console.WriteLine("--Menu Event--");
        Console.WriteLine($"Menu: {e.Menu}");
        Console.WriteLine($"SubMenu: {e.SubMenu}");
        Console.WriteLine($"Online mode: {e.OnlineMode}");
        Console.WriteLine($"Frame Count: {e.FrameCount}");
        Console.WriteLine($"Stage: {e.Stage}");
        Console.WriteLine($"Ready to Start: {e.ReadyToStart}");
        if (e.PlayerStates is not null)
        {
            foreach (var player in e.PlayerStates)
            {
                Console.WriteLine($"--Player {player.PlayerIndex} ({player.ControllerStatus})--");
                if (player.ControllerStatus != MenuControllerStatus.Unplugged)
                {
                    Console.WriteLine($"CursorX: {player.CursorX}");
                    Console.WriteLine($"CursorY: {player.CursorY}");
                    Console.WriteLine($"Character: {player.Character}");
                    Console.WriteLine($"Coin down: {player.CoinDown}");
                }
            }
        }
    }

    private void Connection_OnData(object? sender, byte[] e)
    {
        // Pipe the raw data to the filewriter
        _fileWriter.Write(e);
    }

    private void Connection_OnHandshake(object? sender, ConnectionDetails e)
    {
        Console.WriteLine("Handshake");
    }

    private void Connection_OnMessage(object? sender, CommunicationMessage e)
    {
        Console.WriteLine($"Message: {e.Payload.GetType().Name}");
    }
}
