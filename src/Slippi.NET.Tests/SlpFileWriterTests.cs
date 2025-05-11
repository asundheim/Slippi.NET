using Slippi.NET.Slp;
using Slippi.NET.Slp.EventStream;
using Slippi.NET.Slp.Reader.File;
using Slippi.NET.Slp.Writer;

namespace Slippi.NET.Tests;

public class SlpFileWriterTests
{
    [Fact]
    public void EndingWriter_ShouldWriteDataLengthToFile()
    {
        (int dataLength, string newFilename) = RunSlpFileWriter("slp/finalizedFrame.slp");
        SlpFileReader fileReader = new SlpFileReader(newFilename);
        using (SlpFile slpFile = fileReader.OpenSlpFile())
        {
            int writtenDataLength = slpFile.RawDataLength;
            Assert.Equal(dataLength, writtenDataLength);
        }

        File.Delete(newFilename);
    }

    [Fact]
    public void EndingWriter_ShouldSucceedIfNoDisplayNamesOrConnectCodesArePresent()
    {
        (int dataLength, string newFilename) = RunSlpFileWriter("slp/finalizedFrame.slp");
        using (SlippiGame game = new SlippiGame(newFilename))
        {
            var players = game.GetMetadata()?.Players;
            Assert.NotNull(players);
            foreach (var player in players.Values)
            {
                Assert.Equal(string.Empty, player.Names!.Netplay);
                Assert.Equal(string.Empty, player.Names!.Code);
            }
        }

        File.Delete(newFilename);
    }

    [Fact]
    public void EndingWriter_ShouldWriteNamesIfAvailableInGameStart()
    {
        (int _, string newFileName) = RunSlpFileWriter("slp/displayNameAndConnectCodeInGameStart.slp");
        using (SlippiGame game = new SlippiGame(newFileName))
        {
            var players = game.GetMetadata()?.Players;
            Assert.NotNull(players);

            Assert.Equal(4, players.Count);

            Assert.Equal("ekans", players[0].Names!.Netplay);
            Assert.Equal("EKNS#442", players[0].Names!.Code);

            Assert.Equal("gaR's uncle", players[1].Names!.Netplay);
            Assert.Equal("BAP#666", players[1].Names!.Code);

            Assert.Equal("jmlee337", players[2].Names!.Netplay);
            Assert.Equal("JMLE#166", players[2].Names!.Code);

            Assert.Equal("Mr.SuiSui", players[3].Names!.Netplay);
            Assert.Equal("SUI#244", players[3].Names!.Code);
        }

        File.Delete(newFileName);
    }

    private (int dataLength, string newFileName) RunSlpFileWriter(string testFile)
    {
        var slpInput = new SlpFileReader(testFile);
        using var slpFile = slpInput.OpenSlpFile();
        int dataLength = slpFile.RawDataLength;
        int dataPos = slpFile.RawDataPosition;

        using FileStream testFileStream = new FileStream(testFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 0, useAsync: true);

        using var slpFileWriter = new SlpFileWriter();
        int newPos = PipeMessageSizes(testFileStream, dataPos, slpFileWriter);

        string? newFilename = slpFileWriter.GetCurrentFilename();
        Assert.NotNull(newFilename);

        PipeAllEvents(testFileStream, newPos, dataPos + dataLength, slpFileWriter, slpFile.MessageSizes);

        return (dataLength, newFilename);
    }

    private int PipeMessageSizes(FileStream stream, int start, SlpEventStream writeStream)
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

    private void PipeAllEvents(FileStream stream, int start, int end, SlpEventStream writeStream, Dictionary<int, int> messageSizes)
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
