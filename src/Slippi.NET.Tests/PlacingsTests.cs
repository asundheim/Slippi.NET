namespace Slippi.NET.Tests;

using Slippi.NET;
using Slippi.NET.Stats;

public class PlacingsTests
{
    [Fact]
    public void ShouldReturnEmptyPlacingsForOlderSlpFiles()
    {
        // Arrange
        var game = new SlippiGame("slp/test.slp", new StatOptions());
        var gameEnd = game.GetGameEnd();

        // Act
        var placements = gameEnd?.Placements;

        // Assert
        Assert.NotNull(placements);
        Assert.Equal(4, placements.Count);
        Assert.All(placements, p => Assert.Null(p.Position));
    }

    [Fact]
    public void ShouldAwardWinnerToNonLRASPlayer()
    {
        // Arrange
        var game = new SlippiGame("slp/unranked_game1.slp", new StatOptions());
        var gameEnd = game.GetGameEnd();
        var winners = game.GetWinners();

        // Assert
        Assert.NotNull(gameEnd);
        Assert.NotNull(winners);
        Assert.Single(winners);
        Assert.NotEqual(gameEnd.LrasInitiatorIndex, winners[0].PlayerIndex);
        Assert.Equal(1, winners[0].PlayerIndex);
    }

    [Fact]
    public void ShouldFindWinnerInFreeForAll()
    {
        // Arrange
        var game = new SlippiGame("slp/placementsTest/ffa_1p2p_winner_2p.slp", new StatOptions());
        var winners = game.GetWinners();

        // Assert
        Assert.NotNull(winners);
        Assert.Single(winners);
        Assert.Equal(1, winners[0].PlayerIndex);
        Assert.NotNull(winners[0].Position);
        Assert.Equal(0, winners[0].Position!.Value);
    }

    [Fact]
    public void ShouldReturnPlacingsForTwoPlayerGames()
    {
        // Arrange
        var game = new SlippiGame("slp/placementsTest/ffa_1p2p_winner_2p.slp", new StatOptions());
        var placements = game.GetGameEnd()?.Placements;

        // Assert
        Assert.NotNull(placements);
        Assert.Equal(4, placements.Count);
        Assert.NotNull(placements[0].Position);
        Assert.Equal(1, placements[0].Position!.Value);
        Assert.Equal(0, placements[0].PlayerIndex);
        Assert.NotNull(placements[1].Position);
        Assert.Equal(0, placements[1].Position!.Value);
        Assert.Equal(1, placements[1].PlayerIndex);
    }

    [Fact]
    public void ShouldReturnPlacingsForThreePlayerGames()
    {
        // Arrange
        var game = new SlippiGame("slp/placementsTest/ffa_1p2p3p_winner_3p.slp", new StatOptions());
        var placements = game.GetGameEnd()?.Placements;

        // Assert
        Assert.NotNull(placements);
        Assert.Equal(4, placements.Count);
        Assert.Equal(0, placements[0].PlayerIndex);
        Assert.Equal(1, placements[1].PlayerIndex);
        Assert.Equal(2, placements[2].PlayerIndex);
        Assert.Equal(3, placements[3].PlayerIndex);

        Assert.NotNull(placements[0].Position);
        Assert.Equal(1, placements[0].Position!.Value);
        Assert.NotNull(placements[1].Position);
        Assert.Equal(2, placements[1].Position!.Value);
        Assert.NotNull(placements[2].Position);
        Assert.Equal(0, placements[2].Position!.Value);
        Assert.NotNull(placements[3].Position);
        Assert.Equal(-1, placements[3].Position!.Value);
    }

    [Fact]
    public void ShouldReturnAllWinnersInTeamsMode()
    {
        // Arrange
        var game = new SlippiGame("slp/placementsTest/teams_time_p3_redVSp1p2_blueVSp4_green_winner_blue.slp", new StatOptions());
        var settings = game.GetSettings();
        var winners = game.GetWinners();

        // Assert
        Assert.NotNull(settings);
        Assert.NotNull(winners);
        Assert.Equal(2, winners.Count);
        Assert.Equal(0, winners[0].PlayerIndex);
        Assert.Equal(1, winners[1].PlayerIndex);
        Assert.Equal(1, settings.Players[0].TeamId);
        Assert.Equal(1, settings.Players[1].TeamId);
    }

    [Fact]
    public void ShouldReturnCorrectPlacingsInTimedMode()
    {
        // Arrange
        var game = new SlippiGame("slp/placementsTest/teams_time_p3_redVSp1p2_blueVSp4_green_winner_blue.slp", new StatOptions());
        var settings = game.GetSettings();
        var placements = game.GetGameEnd()?.Placements;

        // Assert
        Assert.NotNull(settings);
        Assert.NotNull(placements);
        Assert.Equal(4, placements.Count);

        Assert.NotNull(placements[0].Position);
        Assert.Equal(1, placements[0].Position!.Value);
        Assert.NotNull(placements[1].Position);
        Assert.Equal(0, placements[1].Position!.Value);
        Assert.NotNull(placements[2].Position);
        Assert.Equal(3, placements[2].Position!.Value);
        Assert.NotNull(placements[3].Position);
        Assert.Equal(2, placements[3].Position!.Value);

        Assert.Equal(1, settings.Players[0].TeamId);
        Assert.Equal(1, settings.Players[1].TeamId);
        Assert.Equal(0, settings.Players[2].TeamId);
        Assert.Equal(2, settings.Players[3].TeamId);
    }

    [Fact]
    public void ShouldReturnCorrectWinnersInTimeout()
    {
        // Arrange
        var game = new SlippiGame("slp/placementsTest/incorrect-winner-timeout.slp", new StatOptions());
        var winners = game.GetWinners();

        // Assert
        Assert.NotNull(winners);
        Assert.Single(winners);
        Assert.Equal(0, winners[0].PlayerIndex);
        Assert.NotNull(winners[0].Position);
        Assert.Equal(0, winners[0].Position!.Value);
    }
}