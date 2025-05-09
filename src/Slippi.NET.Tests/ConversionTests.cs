using Slippi.NET;
using Slippi.NET.Stats;
using Slippi.NET.Stats.Types;
using Xunit;

namespace Slippi.NET.Tests;

public class ConversionTests
{
    [Fact]
    public void ShouldIncludePuffsSing()
    {
        // Arrange
        var game = new SlippiGame("slp/consistencyTest/PuffVFalcon-Sing.slp", new StatOptions());
        var stats = game.GetStats();
        Assert.NotNull(stats);

        var puff = stats.Overall[0];
        var totalDamagePuffDealt = stats.Conversions
            .Where(conversion => conversion.LastHitBy == puff.PlayerIndex)
            .Sum(conversion => conversion.Moves.Sum(move => move.Damage));

        // Assert
        Assert.Equal(puff.TotalDamage, totalDamagePuffDealt);
        Assert.Equal(0, puff.KillCount);
        Assert.Equal(2, puff.ConversionCount);
    }

    [Fact]
    public void ShouldIncludeBowsersCommandGrab()
    {
        // Arrange
        var game = new SlippiGame("slp/consistencyTest/BowsVDK-SB-63.slp", new StatOptions());
        var stats = game.GetStats();
        Assert.NotNull(stats);

        var bowser = stats.Overall[0];
        var totalDamageBowserDealt = stats.Conversions
            .Where(conversion => conversion.LastHitBy == bowser.PlayerIndex)
            .Sum(conversion => conversion.Moves.Sum(move => move.Damage));

        // Assert
        Assert.True(bowser.TotalDamage >= 63);
        Assert.Equal(bowser.TotalDamage, totalDamageBowserDealt);
        Assert.Equal(0, bowser.KillCount);
        Assert.Equal(3, bowser.ConversionCount);
    }

    [Fact]
    public void ShouldIncludeFalconsCommandGrab()
    {
        // Arrange
        var game = new SlippiGame("slp/consistencyTest/FalcVBows-5UB-67.slp", new StatOptions());
        var stats = game.GetStats();
        Assert.NotNull(stats);

        var falcon = stats.Overall[0];
        var totalDamageFalconDealt = stats.Conversions
            .Where(conversion => conversion.LastHitBy == falcon.PlayerIndex)
            .Sum(conversion => conversion.Moves.Sum(move => move.Damage));

        // Assert
        Assert.Equal(falcon.TotalDamage, totalDamageFalconDealt);
        Assert.Equal(0, falcon.KillCount);
        Assert.Equal(3, falcon.ConversionCount);
    }

    [Fact]
    public void ShouldIncludeGanonsCommandGrab()
    {
        // Arrange
        var game = new SlippiGame("slp/consistencyTest/GanonVDK-5UB-73.slp", new StatOptions());
        var stats = game.GetStats();
        Assert.NotNull(stats);

        var ganon = stats.Overall[0];
        var totalDamageGanonDealt = stats.Conversions
            .Where(conversion => conversion.LastHitBy == ganon.PlayerIndex)
            .Sum(conversion => conversion.Moves.Sum(move => move.Damage));

        // Assert
        Assert.Equal(ganon.TotalDamage, totalDamageGanonDealt);
        Assert.Equal(0, ganon.KillCount);
        Assert.Equal(5, ganon.ConversionCount);
    }

    [Fact]
    public void ShouldIncludeKirbysCommandGrab()
    {
        // Arrange
        var game = new SlippiGame("slp/consistencyTest/KirbyVDK-Neutral-17.slp", new StatOptions());
        var stats = game.GetStats();
        Assert.NotNull(stats);

        var kirby = stats.Overall[0];
        var totalDamageKirbyDealt = stats.Conversions
            .Where(conversion => conversion.LastHitBy == kirby.PlayerIndex)
            .Sum(conversion => conversion.Moves.Sum(move => move.Damage));

        // Assert
        Assert.Equal(kirby.TotalDamage, totalDamageKirbyDealt);
        Assert.Equal(0, kirby.KillCount);
        Assert.Equal(3, kirby.ConversionCount);
    }

    [Fact]
    public void ShouldIncludeYoshisCommandGrab()
    {
        // Arrange
        var game = new SlippiGame("slp/consistencyTest/YoshiVDK-Egg-13.slp", new StatOptions());
        var stats = game.GetStats();
        Assert.NotNull(stats);

        var yoshi = stats.Overall[0];
        var totalDamageYoshiDealt = stats.Conversions
            .Where(conversion => conversion.LastHitBy == yoshi.PlayerIndex)
            .Sum(conversion => conversion.Moves.Sum(move => move.Damage));

        // Assert
        Assert.Equal(yoshi.TotalDamage, totalDamageYoshiDealt);
        Assert.Equal(0, yoshi.KillCount);
        Assert.Equal(2, yoshi.ConversionCount);
    }

    [Fact]
    public void ShouldIncludeMewtwosCommandGrab()
    {
        // Arrange
        var game = new SlippiGame("slp/consistencyTest/MewTwoVDK-SB-42.slp", new StatOptions());
        var stats = game.GetStats();
        Assert.NotNull(stats);

        var mewTwo = stats.Overall[0];
        var totalDamageMewTwoDealt = stats.Conversions
            .Where(conversion => conversion.LastHitBy == mewTwo.PlayerIndex)
            .Sum(conversion => conversion.Moves.Sum(move => move.Damage));

        // Assert
        Assert.Equal(mewTwo.TotalDamage, totalDamageMewTwoDealt);
        Assert.Equal(0, mewTwo.KillCount);
        Assert.Equal(1, mewTwo.ConversionCount);
    }

    [Fact]
    public void ShouldNotCreateTwoConversionsForOneKirbyNeutralB()
    {
        // Arrange
        var game = new SlippiGame("slp/consistencyTest/KirbyVMario-nB.slp", new StatOptions());
        var stats = game.GetStats();
        Assert.NotNull(stats);

        var kirby = stats.Overall[0];
        var mario = stats.Overall[1];

        // Assert
        Assert.Equal(1, kirby.ConversionCount);
        Assert.Equal(0, mario.ConversionCount);
    }

    [Fact]
    public void ShouldNotCreateConversionsForDKAerialNeutralBWindup()
    {
        // Arrange
        var game = new SlippiGame("slp/consistencyTest/DKVBows-nB.slp", new StatOptions());
        var stats = game.GetStats();
        Assert.NotNull(stats);

        var dk = stats.Overall[0];
        var bowser = stats.Overall[1];

        // Assert
        Assert.Equal(0, dk.ConversionCount);
        Assert.Equal(0, bowser.ConversionCount);
    }
}