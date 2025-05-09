namespace Slippi.NET.Tests;

using Slippi.NET;
using Slippi.NET.Stats;
using Slippi.NET.Stats.Types;
using Slippi.NET.Types;
using Slippi.NET.Utils;
using Xunit;

public class HomeRunTests
{
    [Fact]
    public void ShouldCorrectlyCalculateDistanceForNegativeDistanceHits()
    {
        // Arrange
        float inGameUnits = -12345.6f;

        // Act
        float homeRunDistance = HomeRunDistance.PositionToHomeRunDistance(inGameUnits, HomeRunDistanceUnits.METERS);

        // Assert
        Assert.Equal(0, homeRunDistance);
    }

    [Fact]
    public void ShouldCorrectlyCalculateDistanceForPositiveDistanceHits()
    {
        // Arrange
        var game = new SlippiGame("slp/homeRun_positive.slp", new StatOptions());

        // Act
        var stadiumStats = game.GetStadiumStats() as HomeRunContestStats;

        // Assert
        Assert.NotNull(stadiumStats);
        Assert.Equal(1070.9f, stadiumStats.DistanceInfo.Distance, 1);
    }

    [Fact]
    public void ShouldCorrectlyCalculateDistanceForJapaneseReplays()
    {
        // Arrange
        var game = new SlippiGame("slp/homeRun_jp.slp", new StatOptions());

        // Act
        var stadiumStats = game.GetStadiumStats() as HomeRunContestStats;

        // Assert
        Assert.NotNull(stadiumStats);
        Assert.Equal(110.8f, stadiumStats.DistanceInfo.Distance, 1);
    }

    [Fact]
    public void ShouldCorrectlySkipProcessingForNonHRCReplays()
    {
        // Arrange
        var game = new SlippiGame("slp/facingDirection.slp", new StatOptions());

        // Act
        var stadiumStats = game.GetStadiumStats() as HomeRunContestStats;

        // Assert
        Assert.Null(stadiumStats);
    }
}
