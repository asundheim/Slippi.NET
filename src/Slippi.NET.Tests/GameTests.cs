using Slippi.NET;
using Slippi.NET.Stats;
using Slippi.NET.Types;
using Xunit;

namespace Slippi.NET.Tests;

public class GameTests
{
    [Fact]
    public void ShouldCorrectlyReturnGameSettings()
    {
        // Arrange
        var game = new SlippiGame("slp/sheik_vs_ics_yoshis.slp", new StatOptions());
        var settings = game.GetSettings();

        // Assert
        Assert.NotNull(settings);
        Assert.Equal(8, settings.StageId);
        Assert.Equal(0x13, settings.Players.First().CharacterId);
        Assert.Equal(0xE, settings.Players.Last().CharacterId);
        Assert.Equal("0.1.0", settings.SlpVersion);
    }

    [Fact]
    public void ShouldCorrectlyReturnStats()
    {
        // Arrange
        var game = new SlippiGame("slp/test.slp", new StatOptions());
        var stats = game.GetStats();

        // Assert
        Assert.NotNull(stats);
        Assert.Equal(3694, stats.LastFrame);

        // Test stocks
        Assert.Equal(5, stats.Stocks.Count);
        Assert.Equal(3694, stats.Stocks.Last().EndFrame);

        // Test conversions
        Assert.Equal(10, stats.Conversions.Count);
        var firstConversion = stats.Conversions.First();
        Assert.Equal(4, firstConversion.Moves.Count);
        Assert.Equal(15, firstConversion.Moves.First().MoveId);
        Assert.Equal(17, firstConversion.Moves.Last().MoveId);

        // Test action counts
        Assert.Equal(16, stats.ActionCounts[0].WavedashCount);
        Assert.Equal(1, stats.ActionCounts[0].WavelandCount);
        Assert.Equal(3, stats.ActionCounts[0].AirDodgeCount);

        // Test attack counts
        Assert.Equal(3, stats.ActionCounts[0].AttackCount.Ftilt);
        Assert.Equal(1, stats.ActionCounts[0].AttackCount.Dash);
        Assert.Equal(4, stats.ActionCounts[0].AttackCount.Fsmash);
        Assert.Equal(4, stats.ActionCounts[0].AttackCount.Bair);

        // Test overall
        Assert.Equal(494, stats.Overall[0].InputCounts.Total);
    }

    [Fact]
    public void ShouldCorrectlyReturnMetadata()
    {
        // Arrange
        var game = new SlippiGame("slp/test.slp", new StatOptions());
        var metadata = game.GetMetadata();

        // Assert
        Assert.NotNull(metadata);
        Assert.Equal("2017-12-18T21:14:14Z", metadata.StartAt);
        Assert.Equal("dolphin", metadata.PlayedOn);
    }

    [Fact]
    public void ShouldCorrectlyReturnFilePath()
    {
        // Arrange
        var game = new SlippiGame("slp/test.slp", new StatOptions());
        var emptyGame = new SlippiGame(Array.Empty<byte>(), new StatOptions());

        // Assert
        Assert.Equal("slp/test.slp", game.GetFilePath());
        Assert.Null(emptyGame.GetFilePath());
    }

    [Fact]
    public void ShouldBeAbleToReadIncompleteSlpFiles()
    {
        // Arrange
        var game = new SlippiGame("slp/incomplete.slp", new StatOptions());

        // Act
        var settings = game.GetSettings();
        game.GetMetadata();
        game.GetStats();

        // Assert
        Assert.NotNull(settings);
        Assert.Equal(2, settings.Players.Count);
    }

    [Fact]
    public void ShouldBeAbleToReadNametags()
    {
        // Arrange
        var game = new SlippiGame("slp/nametags.slp", new StatOptions());
        var game2 = new SlippiGame("slp/nametags2.slp", new StatOptions());
        var game3 = new SlippiGame("slp/nametags3.slp", new StatOptions());

        // Act
        var settings = game.GetSettings();
        var settings2 = game2.GetSettings();
        var settings3 = game3.GetSettings();

        // Assert
        Assert.NotNull(settings);
        Assert.Equal("AMNイ", settings.Players[0].Nametag);
        Assert.Equal("", settings.Players[1].Nametag);

        Assert.NotNull(settings2);
        Assert.Equal("A1=$", settings2.Players[0].Nametag);
        Assert.Equal("か、9@", settings2.Players[1].Nametag);

        Assert.NotNull(settings3);
        Assert.Equal("B  R", settings3.Players[0].Nametag);
        Assert.Equal(".  。", settings3.Players[1].Nametag);
    }

    [Fact]
    public void ShouldBeAbleToReadNetplayNamesAndCodes()
    {
        // Arrange
        var game = new SlippiGame("slp/finalizedFrame.slp", new StatOptions());
        var players = game.GetMetadata()?.Players;

        // Assert
        Assert.NotNull(players);
        Assert.Equal("V", players[0].Names.Netplay);
        Assert.Equal("VA#0", players[0].Names.Code);
        Assert.Equal("Fizzi", players[1].Names.Netplay);
        Assert.Equal("FIZZI#36", players[1].Names.Code);
    }

    [Fact]
    public void ShouldBeAbleToReadConsoleNickname()
    {
        // Arrange
        var game = new SlippiGame("slp/realtimeTest.slp", new StatOptions());

        // Act
        var nickName = game.GetMetadata()?.ConsoleNick;

        // Assert
        Assert.Equal("Day 1", nickName);
    }

    [Fact]
    public void ShouldSupportPalVersion()
    {
        // Arrange
        var palGame = new SlippiGame("slp/pal.slp", new StatOptions());
        var ntscGame = new SlippiGame("slp/ntsc.slp", new StatOptions());

        // Assert
        Assert.True(palGame.GetSettings()?.IsPAL);
        Assert.False(ntscGame.GetSettings()?.IsPAL);
    }

    [Fact]
    public void ShouldCorrectlyDistinguishBetweenDifferentControllerFixes()
    {
        // Arrange
        var game = new SlippiGame("slp/controllerFixes.slp", new StatOptions());
        var settings = game.GetSettings();

        // Assert
        Assert.NotNull(settings);
        Assert.Equal("Dween", settings.Players[0].ControllerFix);
        Assert.Equal("UCF", settings.Players[1].ControllerFix);
        Assert.Equal("None", settings.Players[2].ControllerFix);
    }

    [Fact]
    public void ShouldBeAbleToSupportReadingFromBufferInput()
    {
        // Arrange
        var buf = File.ReadAllBytes("slp/sheik_vs_ics_yoshis.slp");
        var game = new SlippiGame(buf, new StatOptions());
        var settings = game.GetSettings();

        // Assert
        Assert.NotNull(settings);
        Assert.Equal(8, settings.StageId);
        Assert.Equal(0x13, settings.Players.First().CharacterId);
        Assert.Equal(0xE, settings.Players.Last().CharacterId);
    }
}