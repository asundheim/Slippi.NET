namespace Slippi.NET.Tests;

using Slippi.NET;
using Slippi.NET.Stats;

public class DoublesTests
{
    [Fact]
    public void ShouldCorrectlyHandleWhenPlayersAreEliminated()
    {
        // Arrange
        var game = new SlippiGame("slp/doubles.slp", new StatOptions());
        var settings = game.GetSettings();
        var metadata = game.GetMetadata();

        Assert.NotNull(settings);
        Assert.NotNull(metadata);
        Assert.Equal(4, settings.Players.Count);

        int p1ElimFrame = 7754;
        int p1StockStealFrame = 7783;
        int p1ElimFrame2 = 8236;
        int? gameEndFrame = metadata.LastFrame;

        var frames = game.GetFrames();

        // Act & Assert
        // Check that p1 still has a frame when they get eliminated
        Assert.NotNull(frames[p1ElimFrame]?.Players?[0]);

        // Check that eliminated frames are null
        for (int i = p1ElimFrame + 1; i < p1StockStealFrame; i++)
        {
            Assert.True(!(frames[i]?.Players?.ContainsKey(0) ?? true));
        }

        // After the player steals the stock, they should be non-null again
        for (int i = p1StockStealFrame; i <= p1ElimFrame2; i++)
        {
            Assert.NotNull(frames[i]?.Players?[0]);
        }

        // Check that eliminated frames are null again when they lose their last stock
        for (int i = p1ElimFrame2 + 1; i <= gameEndFrame; i++)
        {
            Assert.True(!(frames[i]?.Players?.ContainsKey(0) ?? true));
        }
    }
}