using Slippi.NET;
using Slippi.NET.Stats;
using Slippi.NET.Stats.Types;
using Xunit;

namespace Slippi.NET.Tests;

public class TargetTests
{
    private readonly StatOptions _statOptions = new();

    [Fact]
    public void ShouldCorrectlyCountTargetBreaks()
    {
        // Arrange
        var game = new SlippiGame("slp/BTTDK.slp", _statOptions);
        var stadiumStats = game.GetStadiumStats();

        // Assert
        Assert.NotNull(stadiumStats);
        Assert.Equal("target-test", stadiumStats.Type);

        var targetTestStats = stadiumStats as TargetTestStats;
        Assert.NotNull(targetTestStats);

        var targetsBroken = targetTestStats.TargetBreaks.Count(t => t.FrameDestroyed.HasValue);
        Assert.Equal(10, targetsBroken);
    }

    [Fact]
    public void ShouldCorrectlySkipProcessingForNonHRCReplays()
    {
        // Arrange
        var game = new SlippiGame("slp/facingDirection.slp", _statOptions);
        var stadiumStats = game.GetStadiumStats();

        // Assert
        Assert.Null(stadiumStats);
    }
}