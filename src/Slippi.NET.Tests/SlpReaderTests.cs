using Slippi.NET.Slp.Reader;
using Slippi.NET.Types;
using Slippi.NET.Stats;
using Slippi.NET.Slp.Reader.File;

namespace Slippi.NET.Tests;

public class SlpReaderTests
{
    [Fact]
    public void ShouldReturnSameGameEndObject()
    {
        var game = new SlippiGame("slp/test.slp", new StatOptions());
        var gameEnd = game.GetGameEnd()!;

        var manualGameEnd = GetManualGameEnd("slp/test.slp")!;
        Assert.Equal(gameEnd.GameEndMethod, manualGameEnd.GameEndMethod);
        Assert.Equal(gameEnd.LrasInitiatorIndex, manualGameEnd.LrasInitiatorIndex);
        Assert.Equal(gameEnd.Placements.Count, manualGameEnd.Placements.Count);
    }

    [Fact]
    public void ShouldReturnCorrectPlacingsForTwoPlayerGames()
    {
        var manualGameEnd = GetManualGameEnd("slp/placementsTest/ffa_1p2p_winner_2p.slp")!;
        var placements = manualGameEnd.Placements!;
        Assert.Equal(4, placements.Count);
        Assert.Equal(1, placements[0].Position!.Value); // player in port 1 is on second place
        Assert.Equal(0, placements[0].PlayerIndex);
        Assert.Equal(0, placements[1].Position!.Value); // player in port 2 is on first place
        Assert.Equal(1, placements[1].PlayerIndex);
    }

    [Fact]
    public void ShouldReturnPlacingsForThreePlayerGames()
    {
        var manualGameEnd = GetManualGameEnd("slp/placementsTest/ffa_1p2p3p_winner_3p.slp")!;
        var placements = manualGameEnd.Placements!;
        Assert.NotNull(placements);
        Assert.Equal(4, placements.Count);

        Assert.Equal(0, placements[0].PlayerIndex);
        Assert.Equal(1, placements[1].PlayerIndex);
        Assert.Equal(2, placements[2].PlayerIndex);
        Assert.Equal(3, placements[3].PlayerIndex);

        Assert.Equal(1, placements[0].Position!.Value); // Expect player 1 to be on second place
        Assert.Equal(2, placements[1].Position!.Value); // Expect player 2 to be on third place
        Assert.Equal(0, placements[2].Position!.Value); // Expect player 3 to be first place
        Assert.Equal(-1, placements[3].Position!.Value); // Expect player 4 to not be present

        manualGameEnd = GetManualGameEnd("slp/placementsTest/ffa_1p2p4p_winner_4p.slp")!;
        placements = manualGameEnd.Placements!;
        Assert.NotNull(placements);
        Assert.Equal(4, placements.Count);

        Assert.Equal(0, placements[0].PlayerIndex);
        Assert.Equal(1, placements[1].PlayerIndex);
        Assert.Equal(2, placements[2].PlayerIndex);
        Assert.Equal(3, placements[3].PlayerIndex);

        Assert.Equal(1, placements[0].Position!.Value); // Expect player 1 to be on second place
        Assert.Equal(2, placements[1].Position!.Value); // Expect player 2 to be on third place
        Assert.Equal(-1, placements[2].Position!.Value); // Expect player 3 to not be present
        Assert.Equal(0, placements[3].Position!.Value); // Expect player 4 to be first place
    }

    private static GameEnd? GetManualGameEnd(string filePath)
    {
        var input = new SlpFileReadInput()
        {
            FilePath = filePath
        };

        using var slpFile = SlpReader.OpenSlpFile(input);
        var gameEnd = slpFile.GetGameEnd();
        return gameEnd;
    }
}