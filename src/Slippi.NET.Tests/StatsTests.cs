using Slippi.NET.Stats;
using Slippi.NET.Stats.Types;
using Slippi.NET.Stats.Utils;

namespace Slippi.NET.Tests;

public class StatsTests
{
    private readonly StatOptions _statOptions = new();

    [Fact]
    public void ShouldCorrectlyCalculateLCancelCounts()
    {
        var game = new SlippiGame("slp/lCancel.slp", _statOptions);
        var stats = game.GetStats();

        Assert.NotNull(stats);
        Assert.Equal(3, stats.ActionCounts[0].LCancelCount.Success);
        Assert.Equal(4, stats.ActionCounts[0].LCancelCount.Fail);
        Assert.Equal(5, stats.ActionCounts[1].LCancelCount.Success);
        Assert.Equal(4, stats.ActionCounts[1].LCancelCount.Fail);
    }

    [Fact]
    public void ShouldCountRepeatActionsProperly()
    {
        var game = new SlippiGame("slp/actionEdgeCases.slp", _statOptions);
        var stats = game.GetStats();

        Assert.NotNull(stats);
        Assert.Equal(8, stats.ActionCounts[0].AttackCount.Bair);
        Assert.Equal(4, stats.ActionCounts[0].SpotDodgeCount);
    }

    [Fact]
    public void ShouldCountAngledAttacksProperly()
    {
        var game = new SlippiGame("slp/actionEdgeCases.slp", _statOptions);
        var stats = game.GetStats();

        Assert.NotNull(stats);
        Assert.Equal(3, stats.ActionCounts[0].AttackCount.Ftilt);
        Assert.Equal(3, stats.ActionCounts[0].AttackCount.Fsmash);
    }

    [Fact]
    public void ShouldCountGnwWeirdMovesetCorrectly()
    {
        var game = new SlippiGame("slp/gnwActions.slp", _statOptions);
        var stats = game.GetStats();

        Assert.NotNull(stats);
        Assert.Equal(2, stats.ActionCounts[0].AttackCount.Jab1);
        Assert.Equal(1, stats.ActionCounts[0].AttackCount.Jabm);
        Assert.Equal(1, stats.ActionCounts[0].AttackCount.Ftilt);
        Assert.Equal(1, stats.ActionCounts[0].AttackCount.Utilt);
        Assert.Equal(1, stats.ActionCounts[0].AttackCount.Dtilt);
        Assert.Equal(1, stats.ActionCounts[0].AttackCount.Fsmash);
        Assert.Equal(1, stats.ActionCounts[0].AttackCount.Usmash);
        Assert.Equal(1, stats.ActionCounts[0].AttackCount.Dsmash);
        Assert.Equal(1, stats.ActionCounts[0].AttackCount.Nair);
        Assert.Equal(1, stats.ActionCounts[0].AttackCount.Fair);
        Assert.Equal(1, stats.ActionCounts[0].AttackCount.Bair);
        Assert.Equal(1, stats.ActionCounts[0].AttackCount.Uair);
        Assert.Equal(1, stats.ActionCounts[0].AttackCount.Dair);
        Assert.Equal(2, stats.ActionCounts[0].LCancelCount.Success);
        Assert.Equal(0, stats.ActionCounts[0].LCancelCount.Fail);
    }

    [Fact]
    public void ShouldCountJabsProperly()
    {
        var game = new SlippiGame("slp/actionEdgeCases.slp", _statOptions);
        var stats = game.GetStats();

        Assert.NotNull(stats);
        Assert.Equal(4, stats.ActionCounts[0].AttackCount.Jab1);
        Assert.Equal(3, stats.ActionCounts[0].AttackCount.Jab2);
        Assert.Equal(2, stats.ActionCounts[0].AttackCount.Jab3);
        Assert.Equal(1, stats.ActionCounts[0].AttackCount.Jabm);
    }

    [Fact]
    public void ShouldCountGrabsEvenIfFramePerfectThrow()
    {
        var game = new SlippiGame("slp/actionEdgeCases.slp", _statOptions);
        var stats = game.GetStats();

        Assert.NotNull(stats);
        Assert.Equal(4, stats.ActionCounts[0].GrabCount.Success);
        Assert.Equal(1, stats.ActionCounts[0].GrabCount.Fail);
    }

    [Fact]
    public void ShouldCountDashAttacksCorrectlyDespiteBoostGrabs()
    {
        var game = new SlippiGame("slp/actionEdgeCases.slp", _statOptions);
        var stats = game.GetStats();

        Assert.NotNull(stats);
        Assert.Equal(2, stats.ActionCounts[0].AttackCount.Dash);
    }

    [Fact]
    public void ShouldCorrectlyCalculateThrowCounts()
    {
        var game = new SlippiGame("slp/throwGrab.slp", _statOptions);
        var stats = game.GetStats();

        Assert.NotNull(stats);
        
        Assert.Equal(new ThrowCount() { Up = 1, Forward = 1, Back = 2, Down = 1 }, stats.ActionCounts[1].ThrowCount);
    }

    [Fact]
    public void ShouldCorrectlyCalculateGrabCounts()
    {
        var game = new SlippiGame("slp/throwGrab.slp", _statOptions);
        var stats = game.GetStats();

        Assert.NotNull(stats);
        Assert.Equal(new GrabCount() { Success = 7, Fail = 3 }, stats.ActionCounts[1].GrabCount);
    }

    [Fact]
    public void ShouldIncludeThrowDamageInTotalDamage()
    {
        var game = new SlippiGame("slp/throwGrab.slp", _statOptions);
        var stats = game.GetStats();

        Assert.NotNull(stats);
        Assert.True(stats.Overall[1].TotalDamage >= 75 + 120); // falco
        Assert.True(stats.Overall[0].TotalDamage >= 117 + 153); // marth
    }

    [Fact]
    public void ShouldIncludePummelDamageInTotalDamage()
    {
        var game = new SlippiGame("slp/pummel.slp", _statOptions);
        var stats = game.GetStats();
        Assert.NotNull(stats);

        var marth = stats.Overall[0];
        var sheik = stats.Overall[1];

        Assert.True(marth.TotalDamage >= 14);
        Assert.True(sheik.TotalDamage >= 21);
    }

    [Fact]
    public void ShouldIgnoreBlastZoneMagnifyingGlassDamage()
    {
        var game = new SlippiGame("slp/consistencyTest/Puff-MagnifyingGlass-10.slp", _statOptions);
        var stats = game.GetStats();
        Assert.NotNull(stats);

        var puff = stats.Overall[0];
        var yl = stats.Overall[1];
        float totalDamagePuffDealt = 0f;

        foreach (var conversion in stats.Conversions)
        {
            if (conversion.LastHitBy == puff.PlayerIndex)
            {
                totalDamagePuffDealt += conversion.Moves.Aggregate(0f, (a, b) => a + b.Damage);
            }
        }

        Assert.Equal(puff.TotalDamage, totalDamagePuffDealt);
        Assert.Equal(0, puff.KillCount);
        Assert.Equal(0, puff.ConversionCount);
        Assert.Equal(0, yl.TotalDamage);
    }

    [Fact]
    public void ShouldIgnorePichusSelfDamage()
    {
        var game = new SlippiGame("slp/consistencyTest/PichuVSelf-All-22.slp", _statOptions);
        var stats = game.GetStats();
        Assert.NotNull(stats);

        var pichu = stats.Overall[0];
        var ics = stats.Overall[1];

        var pichuStock = stats.Stocks.Where(s => s.PlayerIndex == pichu.PlayerIndex).First();
        var icsStock = stats.Stocks.Where(s => s.PlayerIndex == ics.PlayerIndex).First();

        float totalDamagePichuDealt = 0f;
        float icsDamageDealt = 0f;
        
        foreach (var conversion in stats.Conversions)
        {
            if (conversion.PlayerIndex == pichu.PlayerIndex)
            {
                icsDamageDealt += conversion.Moves.Aggregate(0f, (a, b) => a + b.Damage);
            }

            if (conversion.PlayerIndex == ics.PlayerIndex)
            {
                totalDamagePichuDealt += conversion.Moves.Aggregate(0f, (a, b) => a + b.Damage);
            }
        }

        // Pichu should have done at least 32% damage
        Assert.True(pichu.TotalDamage >= 32);
        Assert.Equal(pichu.TotalDamage, totalDamagePichuDealt);
        // Pichu's self-damage should not count towards its own total damage dealt
        Assert.NotEqual(pichu.TotalDamage, pichuStock.CurrentPercent + icsStock.CurrentPercent);
        Assert.Equal(0, pichu.KillCount);
        Assert.Equal(0, ics.TotalDamage);
        Assert.Equal(ics.TotalDamage, icsDamageDealt);
        Assert.Equal(3, pichu.ConversionCount);
    }

    [Fact]
    public void ShouldIgnoreNessDamageRecovery()
    {
        var game = new SlippiGame("slp/consistencyTest/NessVFox-Absorb.slp", _statOptions);
        var stats = game.GetStats();
        Assert.NotNull(stats);

        var ness = stats.Overall[0];
        var fox = stats.Overall[1];
        float totalDamageNessDealt = 0;
        float totalDamageFoxDealt = 0;
        foreach (var conversion in stats.Conversions)
        {
            if (conversion.LastHitBy == ness.PlayerIndex)
            {
                totalDamageNessDealt += conversion.Moves.Aggregate(0f, (a, b) => a + b.Damage);
            }

            if (conversion.LastHitBy == fox.PlayerIndex)
            {
                totalDamageFoxDealt += conversion.Moves.Aggregate(0f, (a, b) => a + b.Damage);
            }
        }
        // Ness did no damage to fox
        Assert.Equal(0, ness.TotalDamage);
        Assert.Equal(ness.TotalDamage, totalDamageNessDealt);
        Assert.Equal(0, ness.KillCount);
        Assert.Equal(0, ness.ConversionCount);

        Assert.Equal(fox.TotalDamage, totalDamageFoxDealt);
        Assert.Equal(0, fox.KillCount);
        Assert.Equal(2, fox.ConversionCount);
    }

    [Fact]
    public void ShouldCountTechsOnlyOnce()
    {
        var game = new SlippiGame("slp/techTester.slp", _statOptions);
        var game2 = new SlippiGame("slp/facingDirection.slp", _statOptions);
        var stats = game.GetStats();
        var stats2 = game2.GetStats();

        Assert.NotNull(stats);
        Assert.NotNull(stats2);
        Assert.Equal(new GroundTechCount()
        {
            In = 4,
            Away = 4,
            Neutral = 4,
            Fail = 4,
        }, stats.ActionCounts[0].GroundTechCount);
        // 3 of these tech aways are not facing the opponent
        Assert.Equal(new GroundTechCount()
        {
            In = 1,
            Away = 4,
            Neutral = 4,
            Fail = 11
        }, stats2.ActionCounts[1].GroundTechCount);
        Assert.Equal(new WallTechCount()
        {
            Success = 0,
            Fail = 0,
        }, stats.ActionCounts[1].WallTechCount);
    }

    [Fact]
    public void ShouldCountPeachFsmashCorrectly()
    {
        var game = new SlippiGame("slp/peachFsmash.slp", _statOptions);
        var stats = game.GetStats();

        Assert.NotNull(stats);
        Assert.Equal(4, stats.ActionCounts[0].AttackCount.Fsmash);
    }

    [Fact]
    public void ShouldHandleInvalidStockValues()
    {
        Assert.False(StatsUtils.DidLoseStock(null, null));
    }
}