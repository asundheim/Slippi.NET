using Slippi.NET.Melee.Types;

namespace Slippi.NET.Melee.Data;

public static class Characters
{
    public static readonly Dictionary<Character, CharacterInfo> Lookup = new()
    {
        { Character.CAPTAIN_FALCON, new CharacterInfo(Character.CAPTAIN_FALCON, "Captain Falcon", "Falcon", new List<string> { "Black", "Red", "White", "Green", "Blue" }) },
        { Character.DONKEY_KONG, new CharacterInfo(Character.DONKEY_KONG, "Donkey Kong", "DK", new List<string> { "Black", "Red", "Blue", "Green" }) },
        { Character.FOX, new CharacterInfo(Character.FOX, "Fox", null, new List<string> { "Red", "Blue", "Green" }) },
        { Character.GAME_AND_WATCH, new CharacterInfo(Character.GAME_AND_WATCH, "Mr. Game & Watch", "G&W", new List<string> { "Red", "Blue", "Green" }) },
        { Character.KIRBY, new CharacterInfo(Character.KIRBY, "Kirby", null, new List<string> { "Yellow", "Blue", "Red", "Green", "White" }) },
        { Character.BOWSER, new CharacterInfo(Character.BOWSER, "Bowser", null, new List<string> { "Red", "Blue", "Black" }) },
        { Character.LINK, new CharacterInfo(Character.LINK, "Link", null, new List<string> { "Red", "Blue", "Black", "White" }) },
        { Character.LUIGI, new CharacterInfo(Character.LUIGI, "Luigi", null, new List<string> { "White", "Blue", "Red" }) },
        { Character.MARIO, new CharacterInfo(Character.MARIO, "Mario", null, new List<string> { "Yellow", "Black", "Blue", "Green" }) },
        { Character.MARTH, new CharacterInfo(Character.MARTH, "Marth", null, new List<string> { "Red", "Green", "Black", "White" }) },
        { Character.MEWTWO, new CharacterInfo(Character.MEWTWO, "Mewtwo", null, new List<string> { "Red", "Blue", "Green" }) },
        { Character.NESS, new CharacterInfo(Character.NESS, "Ness", null, new List<string> { "Yellow", "Blue", "Green" }) },
        { Character.PEACH, new CharacterInfo(Character.PEACH, "Peach", null, new List<string> { "Daisy", "White", "Blue", "Green" }) },
        { Character.PIKACHU, new CharacterInfo(Character.PIKACHU, "Pikachu", null, new List<string> { "Red", "Party Hat", "Cowboy Hat" }) },
        { Character.ICE_CLIMBERS, new CharacterInfo(Character.ICE_CLIMBERS, "Ice Climbers", "ICs", new List<string> { "Green", "Orange", "Red" }) },
        { Character.JIGGLYPUFF, new CharacterInfo(Character.JIGGLYPUFF, "Jigglypuff", "Puff", new List<string> { "Red", "Blue", "Headband", "Crown" }) },
        { Character.SAMUS, new CharacterInfo(Character.SAMUS, "Samus", null, new List<string> { "Pink", "Black", "Green", "Purple" }) },
        { Character.YOSHI, new CharacterInfo(Character.YOSHI, "Yoshi", null, new List<string> { "Red", "Blue", "Yellow", "Pink", "Cyan" }) },
        { Character.ZELDA, new CharacterInfo(Character.ZELDA, "Zelda", null, new List<string> { "Red", "Blue", "Green", "White" }) },
        { Character.SHEIK, new CharacterInfo(Character.SHEIK, "Sheik", null, new List<string> { "Red", "Blue", "Green", "White" }) },
        { Character.FALCO, new CharacterInfo(Character.FALCO, "Falco", null, new List<string> { "Red", "Blue", "Green" }) },
        { Character.YOUNG_LINK, new CharacterInfo(Character.YOUNG_LINK, "Young Link", "YLink", new List<string> { "Red", "Blue", "White", "Black" }) },
        { Character.DR_MARIO, new CharacterInfo(Character.DR_MARIO, "Dr. Mario", "Doc", new List<string> { "Red", "Blue", "Green", "Black" }) },
        { Character.ROY, new CharacterInfo(Character.ROY, "Roy", null, new List<string> { "Red", "Blue", "Green", "Yellow" }) },
        { Character.PICHU, new CharacterInfo(Character.PICHU, "Pichu", null, new List<string> { "Red", "Blue", "Green" }) },
        { Character.GANONDORF, new CharacterInfo(Character.GANONDORF, "Ganondorf", "Ganon", new List<string> { "Red", "Blue", "Green", "Purple" }) },
        { Character.MASTER_HAND, new CharacterInfo(Character.MASTER_HAND, "Master Hand", null, []) },
        { Character.WIREFRAME_MALE, new CharacterInfo(Character.WIREFRAME_MALE, "Wireframe (Male)", null, []) },
        { Character.WIREFRAME_FEMALE, new CharacterInfo(Character.WIREFRAME_FEMALE, "Wireframe (Female)", null, []) },
        { Character.GIGA_BOWSER, new CharacterInfo(Character.GIGA_BOWSER, "Gigabowser", null, []) },
        { Character.CRAZY_HAND, new CharacterInfo(Character.CRAZY_HAND, "Crazy Hand", null, []) },
        { Character.SANDBAG, new CharacterInfo(Character.SANDBAG, "Sandbag", null, []) },
        { Character.POPO, new CharacterInfo(Character.POPO, "Popo", null, []) }
    };
}