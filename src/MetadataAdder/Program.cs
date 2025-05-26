using Slippi.NET;
using Slippi.NET.Slp.EventStream;
using Slippi.NET.Slp.Reader.File;
using Slippi.NET.Slp.Writer;
using Slippi.NET.Types;

namespace MetadataAdder;

internal class Program
{
    static void Main(string[] args)
    {
        SlippiGame game = new SlippiGame(args[0]);

        Dictionary<int, PlayerNameInfo> newNames = new Dictionary<int, PlayerNameInfo>();
        foreach (var player in game.GetSettings()!.Players)
        {
            Console.WriteLine($"Input Display Name for Player {player.PlayerIndex} ({player.Character})");
            string displayName = Console.ReadLine() ?? throw new Exception();

            Console.WriteLine($"Input Connect Code for Player {player.PlayerIndex} ({player.Character})");
            string connectCode = Console.ReadLine() ?? throw new Exception();

            newNames[player.PlayerIndex] = new PlayerNameInfo()
            {
                Code = connectCode,
                Netplay = displayName
            };
        }

        Console.WriteLine("Writing metadata...");
        RunSlpFileWriter(args[0], newNames);

        Console.WriteLine("Done");
    }

    private static void RunSlpFileWriter(string testFile, Dictionary<int, PlayerNameInfo> names)
    {
        var slpInput = new SlpFileReader(testFile);
        using var slpFile = slpInput.OpenSlpFile();
        int dataLength = slpFile.RawDataLength;
        int dataPos = slpFile.RawDataPosition;

        using FileStream testFileStream = new FileStream(testFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 0, useAsync: true);

        string oldFileName = Path.GetFileNameWithoutExtension(testFile);
        using SlpFileWriter slpFileWriter = new SlpFileWriter(new SlpFileWriterSettings()
        {
            FolderPath = Path.GetDirectoryName(testFile)!,
            MakeNewFileName = (folder, date) => Path.Join(folder, $"{oldFileName}-NEW.slp"),
            PlayerNameOverrides = names
        });

        int newPos = PipeMessageSizes(testFileStream, dataPos, slpFileWriter);

        string? newFilename = slpFileWriter.GetCurrentFilename();

        PipeAllEvents(testFileStream, newPos, dataPos + dataLength, slpFileWriter, slpFile.MessageSizes);
    }

    private static int PipeMessageSizes(FileStream stream, int start, SlpEventStream writeStream)
    {
        int pos = start;

        Span<byte> commandByteBuffer = stackalloc byte[2];
        stream.Seek(pos, SeekOrigin.Begin);
        stream.ReadExactly(commandByteBuffer);
        int length = commandByteBuffer[1] + 1;

        Span<byte> buffer = stackalloc byte[length];
        stream.Seek(pos, SeekOrigin.Begin);
        stream.ReadExactly(buffer);

        pos += length;
        writeStream.Write(buffer);

        return pos;
    }

    private static void PipeAllEvents(FileStream stream, int start, int end, SlpEventStream writeStream, Dictionary<int, int> messageSizes)
    {
        int pos = start;
        while (pos < end)
        {
            Span<byte> commandByteBuffer = new byte[1];
            stream.Seek(pos, SeekOrigin.Begin);
            stream.ReadExactly(commandByteBuffer);
            int length = messageSizes[commandByteBuffer[0]] + 1;

            Span<byte> buffer = new byte[length];
            stream.Seek(pos, SeekOrigin.Begin);
            stream.ReadExactly(buffer);

            pos += length;
            writeStream.Write(buffer);
        }
    }
}
