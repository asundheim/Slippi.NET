namespace Slippi.NET.Tests;

using Slippi.NET;
using Slippi.NET.Stats;
using Xunit;

public class ItemTests
{
    [Fact]
    public void ShouldMonotonicallyIncrementItemSpawnId()
    {
        // Arrange
        var game = new SlippiGame("slp/itemExport.slp", new StatOptions());
        var frames = game.GetFrames();

        int lastSpawnId = -1;

        // Act & Assert
        Assert.NotNull(frames);
        Assert.NotEmpty(frames.Values);
        foreach (var frame in frames.Values)
        {
            if (frame.Items != null)
            {
                foreach (var item in frame.Items)
                {
                    if (lastSpawnId < item.SpawnId)
                    {
                        Assert.NotNull(item?.SpawnId);
                        Assert.Equal(lastSpawnId + 1, (int)item.SpawnId.Value);
                        lastSpawnId = (int)item.SpawnId.Value;
                    }
                }
            }
        }
    }

    [Fact]
    public void ShouldHaveValidOwnerValues()
    {
        // Arrange
        var game = new SlippiGame("slp/itemExport.slp", new StatOptions());
        var frames = game.GetFrames();

        // Act & Assert
        Assert.NotNull(frames);
        Assert.NotEmpty(frames.Values);
        foreach (var frame in frames.Values)
        {
            if (frame.Items != null)
            {
                foreach (var item in frame.Items)
                {
                    // The owner must be between -1 and 3
                    Assert.NotNull(item.Owner);
                    Assert.InRange(item.Owner.Value, -1, 3);
                }
            }
        }
    }
}