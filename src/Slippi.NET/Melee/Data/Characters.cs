using Slippi.NET.Melee.Types;

namespace Slippi.NET.Melee.Data;

public static class Characters
{
    public static readonly Dictionary<Character, CharacterInfo> Lookup = new()
    {
        { Character.CaptainFalcon, new CharacterInfo(Character.CaptainFalcon, "Captain Falcon", "Falcon", new List<string> { "Black", "Red", "White", "Green", "Blue" }) },
        { Character.DonkeyKong, new CharacterInfo(Character.DonkeyKong, "Donkey Kong", "DK", new List<string> { "Black", "Red", "Blue", "Green" }) },
        { Character.Fox, new CharacterInfo(Character.Fox, "Fox", null, new List<string> { "Red", "Blue", "Green" }) },
        { Character.GameAndWatch, new CharacterInfo(Character.GameAndWatch, "Mr. Game & Watch", "G&W", new List<string> { "Red", "Blue", "Green" }) },
        { Character.Kirby, new CharacterInfo(Character.Kirby, "Kirby", null, new List<string> { "Yellow", "Blue", "Red", "Green", "White" }) },
        { Character.Bowser, new CharacterInfo(Character.Bowser, "Bowser", null, new List<string> { "Red", "Blue", "Black" }) },
        { Character.Link, new CharacterInfo(Character.Link, "Link", null, new List<string> { "Red", "Blue", "Black", "White" }) },
        { Character.Luigi, new CharacterInfo(Character.Luigi, "Luigi", null, new List<string> { "White", "Blue", "Red" }) },
        { Character.Mario, new CharacterInfo(Character.Mario, "Mario", null, new List<string> { "Yellow", "Black", "Blue", "Green" }) },
        { Character.Marth, new CharacterInfo(Character.Marth, "Marth", null, new List<string> { "Red", "Green", "Black", "White" }) },
        { Character.Mewtwo, new CharacterInfo(Character.Mewtwo, "Mewtwo", null, new List<string> { "Red", "Blue", "Green" }) },
        { Character.Ness, new CharacterInfo(Character.Ness, "Ness", null, new List<string> { "Yellow", "Blue", "Green" }) },
        { Character.Peach, new CharacterInfo(Character.Peach, "Peach", null, new List<string> { "Daisy", "White", "Blue", "Green" }) },
        { Character.Pikachu, new CharacterInfo(Character.Pikachu, "Pikachu", null, new List<string> { "Red", "Party Hat", "Cowboy Hat" }) },
        { Character.IceClimbers, new CharacterInfo(Character.IceClimbers, "Ice Climbers", "ICs", new List<string> { "Green", "Orange", "Red" }) },
        { Character.JigglyPuff, new CharacterInfo(Character.JigglyPuff, "Jigglypuff", "Puff", new List<string> { "Red", "Blue", "Headband", "Crown" }) },
        { Character.Samus, new CharacterInfo(Character.Samus, "Samus", null, new List<string> { "Pink", "Black", "Green", "Purple" }) },
        { Character.Yoshi, new CharacterInfo(Character.Yoshi, "Yoshi", null, new List<string> { "Red", "Blue", "Yellow", "Pink", "Cyan" }) },
        { Character.Zelda, new CharacterInfo(Character.Zelda, "Zelda", null, new List<string> { "Red", "Blue", "Green", "White" }) },
        { Character.Sheik, new CharacterInfo(Character.Sheik, "Sheik", null, new List<string> { "Red", "Blue", "Green", "White" }) },
        { Character.Falco, new CharacterInfo(Character.Falco, "Falco", null, new List<string> { "Red", "Blue", "Green" }) },
        { Character.YoungLink, new CharacterInfo(Character.YoungLink, "Young Link", "YLink", new List<string> { "Red", "Blue", "White", "Black" }) },
        { Character.DrMario, new CharacterInfo(Character.DrMario, "Dr. Mario", "Doc", new List<string> { "Red", "Blue", "Green", "Black" }) },
        { Character.Roy, new CharacterInfo(Character.Roy, "Roy", null, new List<string> { "Red", "Blue", "Green", "Yellow" }) },
        { Character.Pichu, new CharacterInfo(Character.Pichu, "Pichu", null, new List<string> { "Red", "Blue", "Green" }) },
        { Character.Ganondorf, new CharacterInfo(Character.Ganondorf, "Ganondorf", "Ganon", new List<string> { "Red", "Blue", "Green", "Purple" }) },
        { Character.MasterHand, new CharacterInfo(Character.MasterHand, "Master Hand", null, []) },
        { Character.WireframeMale, new CharacterInfo(Character.WireframeMale, "Wireframe (Male)", null, []) },
        { Character.WireframeFemale, new CharacterInfo(Character.WireframeFemale, "Wireframe (Female)", null, []) },
        { Character.GigaBowser, new CharacterInfo(Character.GigaBowser, "Gigabowser", null, []) },
        { Character.CrazyHand, new CharacterInfo(Character.CrazyHand, "Crazy Hand", null, []) },
        { Character.Sandbag, new CharacterInfo(Character.Sandbag, "Sandbag", null, []) },
        { Character.Popo, new CharacterInfo(Character.Popo, "Popo", null, []) }
    };
}