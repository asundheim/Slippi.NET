using Slippi.NET.Slp.EventStream;
using Slippi.NET.Slp.EventStream.Types;
using Slippi.NET.Slp.Parser;
using Slippi.NET.Slp.Parser.Types;
using Slippi.NET.Stats;
using Slippi.NET.Types;

namespace Slippi.NET.Tests;

public class RealtimeTests
{
    [Fact]
    public void ReadingLastFinalizedFrameFromSlpStream_ShouldNeverDecrease()
    {
        const string testFile = "slp/finalizedFrame.slp";
        var stream = new SlpEventStream(new SlpStreamSettings() { Mode = SlpStreamModes.MANUAL });
        var parser = new SlpParser(new SlpParserOptions());

        int lastFinalizedFrame = (int)Frames.FIRST - 1;
        int parserLastFinalizedFrame = (int)Frames.FIRST - 1;

        // The game mode should be online
        var game = new SlippiGame(testFile, new StatOptions());
        var settings = game.GetSettings();
        Assert.Equal(GameMode.ONLINE, settings?.GameMode);

        stream.OnCommand += (sender, args) =>
        {
            parser.HandleCommand(args.Command, args.Payload);
            if (args.Command == Command.FRAME_BOOKEND)
            {
                var payload = args.Payload as FrameBookendPayload;
                Assert.NotNull(payload);

                var bookend = payload.FrameBookend;
                Assert.NotNull(bookend.LatestFinalizedFrame);
                Assert.NotEqual((int)Frames.FIRST - 1, bookend.LatestFinalizedFrame);
                Assert.True(bookend.LatestFinalizedFrame >= lastFinalizedFrame);
                Assert.True(bookend.LatestFinalizedFrame >= bookend.Frame - SlpParser.MAX_ROLLBACK_FRAMES);
                lastFinalizedFrame = bookend.LatestFinalizedFrame.Value;
            }
        };

        parser.OnFinalizedFrame += (sender, frameEntry) =>
        {
            Assert.NotNull(frameEntry);
            Assert.NotNull(frameEntry.Frame);
            Assert.NotEqual(parserLastFinalizedFrame, frameEntry.Frame);
            Assert.Equal(parserLastFinalizedFrame + 1, frameEntry.Frame);
            parserLastFinalizedFrame = frameEntry.Frame.Value;
        };

        PipeFileContents(testFile, stream);

        // The last finalized frame should be the same as what's recorded in the metadata
        var metadata = game.GetMetadata();
        Assert.NotNull(metadata);
        Assert.Equal(metadata.LastFrame, lastFinalizedFrame);
    }

    [Fact]
    public void ReadingFinalizedFramesFromSlpParser_ShouldSupportOlderSlpFilesWithoutFrameBookend()
    {
        const string testFile = "slp/sheik_vs_ics_yoshis.slp";
        var stream = new SlpEventStream(new SlpStreamSettings() { Mode = SlpStreamModes.MANUAL });
        var parser = new SlpParser(new SlpParserOptions());

        int lastFinalizedFrame = (int)Frames.FIRST - 1;

        parser.OnFinalizedFrame += (sender, frameEntry) =>
        {
            Assert.NotNull(frameEntry);
            Assert.NotNull(frameEntry.Frame);
            Assert.NotEqual(lastFinalizedFrame, frameEntry.Frame);
            Assert.Equal(lastFinalizedFrame + 1, frameEntry.Frame);
            lastFinalizedFrame = frameEntry.Frame.Value;
        };

        stream.OnCommand += (sender, args) =>
        {
            parser.HandleCommand(args.Command, args.Payload);
        };

        PipeFileContents(testFile, stream);

        var game = new SlippiGame(testFile, new StatOptions());
        var metadata = game.GetMetadata();
        Assert.NotNull(metadata);
        var lastFrame = metadata.LastFrame ?? game.GetLatestFrame()!.Frame;
        Assert.Equal(lastFrame, lastFinalizedFrame);
    }

    [Fact]
    public async Task ReadingFinalizedFramesFromSlpParser_ShouldOnlyIncrease()
    {
        const string testFile = "slp/finalizedFrame.slp";
        var stream = new SlpEventStream(new SlpStreamSettings() { Mode = SlpStreamModes.MANUAL });
        var parser = new SlpParser(new SlpParserOptions());

        int lastFinalizedFrame = (int)Frames.FIRST - 1;

        parser.OnFinalizedFrame += (sender, frameEntry) =>
        {
            Assert.NotNull(frameEntry);
            Assert.NotNull(frameEntry.Frame);
            Assert.NotEqual(lastFinalizedFrame, frameEntry.Frame);
            Assert.Equal(lastFinalizedFrame + 1, frameEntry.Frame);
            lastFinalizedFrame = frameEntry.Frame.Value;
        };

        stream.OnCommand += (sender, args) =>
        {
            parser.HandleCommand(args.Command, args.Payload);
        };

        PipeFileContents(testFile, stream);

        var game = new SlippiGame(testFile, new StatOptions());
        var metadata = game.GetMetadata();
        Assert.NotNull(metadata);
        Assert.Equal(metadata.LastFrame, lastFinalizedFrame);
    }

    [Fact]
    public void ReadConsoleReplay()
    {
        const string testFile = "slp/console.slp";
        var stream = new SlpEventStream(new SlpStreamSettings() { Mode = SlpStreamModes.MANUAL });
        var parser = new SlpParser(new SlpParserOptions());
        stream.OnCommand += (sender, args) =>
        {
            parser.HandleCommand(args.Command, args.Payload);
        };
        PipeFileContents(testFile, stream);
    }

    private static void PipeFileContents(string filename, SlpEventStream destination)
    {
        using var readStream = new FileStream(filename, FileMode.Open, FileAccess.Read);

        // This ensures we buffer correctly and we're not off by 1 anywhere.
        // Configurable for testing purposes.
        const int chunkSize = 1024;
        Span<byte> buffer = stackalloc byte[chunkSize];
        while (readStream.Position < readStream.Length)
        {
            if (readStream.Length - readStream.Position > chunkSize)
            {
                readStream.ReadExactly(buffer);
                destination.Write(buffer);
            }
            else
            {
                Span<byte> remainder = stackalloc byte[(int)(readStream.Length - readStream.Position)];
                readStream.ReadExactly(remainder);
                destination.Write(remainder);

                return;
            }
        }
    }
}