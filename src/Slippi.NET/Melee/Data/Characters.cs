using Slippi.NET.Melee.Types;

namespace Slippi.NET.Melee.Data;

public static class Characters
{
    public static readonly Dictionary<int, CharacterInfo> Lookup = new()
    {
        { 0, new CharacterInfo(0, "Captain Falcon", "Falcon", new List<string> { "Black", "Red", "White", "Green", "Blue" }) },
        { 1, new CharacterInfo(1, "Donkey Kong", "DK", new List<string> { "Black", "Red", "Blue", "Green" }) },
        { 2, new CharacterInfo(2, "Fox", null, new List<string> { "Red", "Blue", "Green" }) },
        { 3, new CharacterInfo(3, "Mr. Game & Watch", "G&W", new List<string> { "Red", "Blue", "Green" }) },
        { 4, new CharacterInfo(4, "Kirby", null, new List<string> { "Yellow", "Blue", "Red", "Green", "White" }) },
        { 5, new CharacterInfo(5, "Bowser", null, new List<string> { "Red", "Blue", "Black" }) },
        { 6, new CharacterInfo(6, "Link", null, new List<string> { "Red", "Blue", "Black", "White" }) },
        { 7, new CharacterInfo(7, "Luigi", null, new List<string> { "White", "Blue", "Red" }) },
        { 8, new CharacterInfo(8, "Mario", null, new List<string> { "Yellow", "Black", "Blue", "Green" }) },
        { 9, new CharacterInfo(9, "Marth", null, new List<string> { "Red", "Green", "Black", "White" }) },
        { 10, new CharacterInfo(10, "Mewtwo", null, new List<string> { "Red", "Blue", "Green" }) },
        { 11, new CharacterInfo(11, "Ness", null, new List<string> { "Yellow", "Blue", "Green" }) },
        { 12, new CharacterInfo(12, "Peach", null, new List<string> { "Daisy", "White", "Blue", "Green" }) },
        { 13, new CharacterInfo(13, "Pikachu", null, new List<string> { "Red", "Party Hat", "Cowboy Hat" }) },
        { 14, new CharacterInfo(14, "Ice Climbers", "ICs", new List<string> { "Green", "Orange", "Red" }) },
        { 15, new CharacterInfo(15, "Jigglypuff", "Puff", new List<string> { "Red", "Blue", "Headband", "Crown" }) },
        { 16, new CharacterInfo(16, "Samus", null, new List<string> { "Pink", "Black", "Green", "Purple" }) },
        { 17, new CharacterInfo(17, "Yoshi", null, new List<string> { "Red", "Blue", "Yellow", "Pink", "Cyan" }) },
        { 18, new CharacterInfo(18, "Zelda", null, new List<string> { "Red", "Blue", "Green", "White" }) },
        { 19, new CharacterInfo(19, "Sheik", null, new List<string> { "Red", "Blue", "Green", "White" }) },
        { 20, new CharacterInfo(20, "Falco", null, new List<string> { "Red", "Blue", "Green" }) },
        { 21, new CharacterInfo(21, "Young Link", "YLink", new List<string> { "Red", "Blue", "White", "Black" }) },
        { 22, new CharacterInfo(22, "Dr. Mario", "Doc", new List<string> { "Red", "Blue", "Green", "Black" }) },
        { 23, new CharacterInfo(23, "Roy", null, new List<string> { "Red", "Blue", "Green", "Yellow" }) },
        { 24, new CharacterInfo(24, "Pichu", null, new List<string> { "Red", "Blue", "Green" }) },
        { 25, new CharacterInfo(25, "Ganondorf", "Ganon", new List<string> { "Red", "Blue", "Green", "Purple" }) },
        { 26, new CharacterInfo(26, "Master Hand", null, []) },
        { 27, new CharacterInfo(27, "Wireframe (Male)", null, []) },
        { 28, new CharacterInfo(28, "Wireframe (Female)", null, []) },
        { 29, new CharacterInfo(29, "Gigabowser", null, []) },
        { 30, new CharacterInfo(30, "Crazy Hand", null, []) },
        { 31, new CharacterInfo(31, "Sandbag", null, []) },
        { 32, new CharacterInfo(32, "Popo", null, []) }
    };
}