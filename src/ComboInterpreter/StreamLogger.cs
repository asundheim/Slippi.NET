using Slippi.NET.Console;
using Slippi.NET.Console.Types;
using Slippi.NET.Slp.Reader.File;
using Slippi.NET.Slp.Writer;
using Slippi.NET.Types;
using System.Diagnostics;
using System.Text;

namespace ComboInterpreter;
internal class StreamLogger
{
    public static void Main(string[] args)
    {
        using DolphinConnection connection = new DolphinConnection();
        using SlpFileWriter fileWriter = new SlpFileWriter(new SlpFileWriterSettings() { FolderPath = Path.GetTempPath() });

        connection.OnHandshake += (object? sender, ConnectionDetails args) =>
        {
            Console.WriteLine("Connected");
        };

        connection.OnData += (object? sender, byte[] data) =>
        {
            fileWriter.Write(data);
        };

        CancellationTokenSource cts = new CancellationTokenSource();

        fileWriter.OnNewFile += (object? sender, string path) =>
        {
            cts?.Dispose();
            cts = new CancellationTokenSource();

            _ = Task.Run(() =>
            {
                FoxComboInterpreter comboBot = new FoxComboInterpreter(path, "george seinfeld", "D#345");
                _ = Task.Run(async () => await comboBot.WaitForLiveGameEndAsync());

                ProcessInterpretedCombos(comboBot, cts.Token);
            });
        };

        fileWriter.OnFileComplete += (object? sender, string path) =>
        {
            cts.Cancel();
        };

        connection.Connect("127.0.0.1", (int)Ports.Default, true, 30_000);
        Console.ReadLine();
    }

    private static void ProcessInterpretedCombos(FoxComboInterpreter comboBot, CancellationToken cancellation)
    {
        bool activeLine = false;
        int sameActionCount = 1;
        bool continuation = false;
        string currentLine = string.Empty;
        Stopwatch s = new Stopwatch();
        s.Start();
        while (!cancellation.IsCancellationRequested)
        {
            var combo = comboBot.ComboStream.Take(cancellation);
            s.Stop();
            if (s.ElapsedMilliseconds >= 450 && activeLine)
            {
                if (!continuation)
                {
                    //Console.WriteLine();
                    currentLine = string.Empty;
                    activeLine = false;
                }
                else
                {
                    s.Restart();
                }
            }

            StringBuilder sb = new StringBuilder();

            sb.Append(combo.DisplayName);
            if (combo.HasContinuation)
            {
                sb.Append(" + ");
                continuation = true;
            }
            else
            {
                sb.Append(' ');
                continuation = false;
            }

            string result = sb.ToString();

            if (currentLine.Length >= result.Length && currentLine.EndsWith(result))
            {
                int parensIndex = currentLine.LastIndexOf(result) + result.Length;
                if (parensIndex <= Console.CursorLeft)
                {
                    //Console.SetCursorPosition(parensIndex, Console.CursorTop);
                    sameActionCount++;
                    //Console.Write($"({sameActionCount}) ");
                }
                else
                {
                    sameActionCount = 1;
                    //Console.Write(sb.ToString());
                    currentLine += result;
                }
            }
            else
            {
                sameActionCount = 1;
                //Console.Write(sb.ToString());
                currentLine += result;
            }

            if (combo.EndsCombo || combo.DisplayName == "Dash")
            {
                //Console.WriteLine();
                currentLine = string.Empty;
                activeLine = false;
                s.Restart();
            }
            else
            {
                activeLine = true;
                s.Restart();
            }
        }
    }
}
