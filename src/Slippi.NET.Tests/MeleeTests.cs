namespace Slippi.NET.Tests;

using Slippi.NET.Melee;
using Slippi.NET.Melee.Types;

public class MeleeTests
{
    private static CharacterInfo _foxCharacter = new CharacterInfo()
    {
        Id = 2,
        Name = "Fox",
        ShortName = "Fox",
        Colors = ["Default", "Red", "Blue", "Green"],
    };

    private static CharacterInfo _unknownCharacter = new CharacterInfo()
    {
        Id = -1,
        Name = "Unknown Character",
        ShortName = "Unknown",
        Colors = ["Default"],
    };

    private static Move _miscMove = new Move(1, "Miscellaneous", "misc");
    private static Move _unknownMove = new Move(-1, "Unknown Move", "unknown");

    private static StageInfo _venomStage = new StageInfo(22, "Venom");
    private static StageInfo _unknownStage = new StageInfo(-1, "Unknown Stage");

    [Fact]
    public void ShouldReturnExpectedDeathDirections()
    {
        // Act & Assert
        Assert.Equal("down", AnimationUtils.GetDeathDirection(0));
        Assert.Equal("left", AnimationUtils.GetDeathDirection(1));
        Assert.Equal("right", AnimationUtils.GetDeathDirection(2));
        Assert.Equal("up", AnimationUtils.GetDeathDirection(3));
        Assert.Null(AnimationUtils.GetDeathDirection(1234));
    }

    [Fact]
    public void ShouldReturnExpectedCharacterInfo()
    {
        var foxCharacter = CharacterUtils.GetCharacterInfo(2);
        Assert.Equal(_foxCharacter.ShortName, foxCharacter.ShortName);
        Assert.Equal(_foxCharacter.Name, foxCharacter.Name);
        Assert.Equal(_foxCharacter.Id, foxCharacter.Id);
        Assert.Equal(_foxCharacter.Colors.Count, foxCharacter.Colors.Count);
        for (int i = 0; i < _foxCharacter.Colors.Count; i++)
        {
            Assert.Equal(_foxCharacter.Colors[i], foxCharacter.Colors[i]);
        }
    }

    [Fact]
    public void ShouldHandleUnknownCharacters()
    {
        var character = CharacterUtils.GetCharacterInfo(69);
        Assert.Equal(_unknownCharacter.ShortName, character.ShortName);
        Assert.Equal(_unknownCharacter.Name, character.Name);
        Assert.Equal(_unknownCharacter.Id, character.Id);
        Assert.Equal(_unknownCharacter.Colors.Count, character.Colors.Count);
        for (int i = 0; i < _unknownCharacter.Colors.Count; i++)
        {
            Assert.Equal(_unknownCharacter.Colors[i], character.Colors[i]);
        }

        Assert.Equal(_unknownCharacter.ShortName, CharacterUtils.UnknownCharacter.ShortName);
        Assert.Equal(_unknownCharacter.Name, CharacterUtils.UnknownCharacter.Name);
        Assert.Equal(_unknownCharacter.Id, CharacterUtils.UnknownCharacter.Id);
        Assert.Equal(_unknownCharacter.Colors.Count, CharacterUtils.UnknownCharacter.Colors.Count);
        for (int i = 0; i < _unknownCharacter.Colors.Count; i++)
        {
            Assert.Equal(_unknownCharacter.Colors[i], CharacterUtils.UnknownCharacter.Colors[i]);
        }
    }

    [Fact]
    public void ShouldReturnCorrectCharacterShortName()
    {
        Assert.Equal(_foxCharacter.ShortName, CharacterUtils.GetCharacterShortName(2));
    }

    [Fact]
    public void ShouldReturnCorrectCharacterName()
    {
        Assert.Equal(_foxCharacter.Name, CharacterUtils.GetCharacterName(2));
    }

    [Fact]
    public void ShouldReturnCorrectCharacterColor()
    {
        Assert.Equal(_foxCharacter.Colors[0], CharacterUtils.GetCharacterColorName(2, 0));
    }

    [Fact]
    public void ShouldReturnDefaultColorIfColorDoesNotExist()
    {
        Assert.Equal(_foxCharacter.Colors[0], CharacterUtils.GetCharacterColorName(2, 10));
    }

    [Fact]
    public void ShouldReturnAllCharacters()
    {
        var allFoxCharacter = CharacterUtils.GetAllCharacters()[2];

        Assert.Equal(_foxCharacter.ShortName, allFoxCharacter.ShortName);
        Assert.Equal(_foxCharacter.Name, allFoxCharacter.Name);
        Assert.Equal(_foxCharacter.Id, allFoxCharacter.Id);
        Assert.Equal(_foxCharacter.Colors.Count, allFoxCharacter.Colors.Count);
        for (int i = 0; i < _foxCharacter.Colors.Count; i++)
        {
            Assert.Equal(_foxCharacter.Colors[i], allFoxCharacter.Colors[i]);
        }
    }

    [Fact]
    public void ShouldReturnCorrectMoveFromId()
    {
        Assert.Equal(_miscMove, MoveUtils.GetMoveInfo(1));
    }

    [Fact]
    public void ShouldHandleUnknownMoves()
    {
        Assert.Equal(_unknownMove, MoveUtils.GetMoveInfo(69));
        Assert.Equal(_unknownMove, MoveUtils.UnknownMove);
    }

    [Fact]
    public void ShouldReturnCorrectMoveShortName()
    {
        Assert.Equal("edge", MoveUtils.GetMoveShortName(62));
    }

    [Fact]
    public void ShouldReturnCorrectMoveName()
    {
        Assert.Equal("Edge Attack", MoveUtils.GetMoveName(62));
    }

    [Fact]
    public void ShouldReturnCorrectStageFromId()
    {
        Assert.Equal(_venomStage, StageUtils.GetStageInfo(22));
    }

    [Fact]
    public void ShouldHandleUnknownStages()
    {
        Assert.Equal(_unknownStage, StageUtils.GetStageInfo(69));
        Assert.Equal(_unknownStage, StageUtils.UnknownStage);
    }

    [Fact]
    public void ShouldReturnCorrectStageNameFromId()
    {
        Assert.Equal(_venomStage.Name, StageUtils.GetStageName(22));
    }
}