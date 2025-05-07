using Slippi.NET.Melee.Data;
using Slippi.NET.Melee.Types;

namespace Slippi.NET.Melee;

public static class CharacterUtils
{
    private const string DefaultColor = "Default";

    public static readonly CharacterInfo UnknownCharacter = new CharacterInfo
    {
        Id = -1,
        Name = "Unknown Character",
        ShortName = "Unknown",
        Colors = new List<string> { DefaultColor }
    };

    /// <summary>
    /// Generates character information based on the provided ID and data.
    /// </summary>
    /// <param name="id">The character ID.</param>
    /// <param name="data">The character data.</param>
    /// <returns>A <see cref="CharacterInfo"/> object.</returns>
    private static CharacterInfo GenerateCharacterInfo(int id, CharacterInfo? data)
    {
        if (data is null)
        {
            return UnknownCharacter;
        }

        return new CharacterInfo
        {
            Id = id,
            Name = data.Name,
            ShortName = data.ShortName ?? data.Name,
            Colors = [DefaultColor, .. data.Colors]
        };
    }

    /// <summary>
    /// Gets all characters as a list of <see cref="CharacterInfo"/>.
    /// </summary>
    /// <returns>A sorted list of all characters.</returns>
    public static List<CharacterInfo> GetAllCharacters()
    {
        return Characters.Lookup
            .Select(entry => GenerateCharacterInfo(entry.Key, entry.Value))
            .OrderBy(character => character.Id)
            .ToList();
    }

    /// <summary>
    /// Gets character information for a specific character ID.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <returns>A <see cref="CharacterInfo"/> object.</returns>
    public static CharacterInfo GetCharacterInfo(int characterId)
    {
        Characters.Lookup.TryGetValue(characterId, out var data);
        return GenerateCharacterInfo(characterId, data);
    }

    /// <summary>
    /// Gets the short name of a character based on its ID.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <returns>The short name of the character.</returns>
    public static string? GetCharacterShortName(int characterId)
    {
        return GetCharacterInfo(characterId).ShortName;
    }

    /// <summary>
    /// Gets the name of a character based on its ID.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <returns>The name of the character.</returns>
    public static string GetCharacterName(int characterId)
    {
        return GetCharacterInfo(characterId).Name;
    }

    /// <summary>
    /// Gets the color name of a character based on its ID and color index.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <param name="characterColor">The color index.</param>
    /// <returns>The color name of the character.</returns>
    public static string GetCharacterColorName(int characterId, int characterColor)
    {
        var character = GetCharacterInfo(characterId);
        if (characterColor >= 0 && characterColor < character.Colors.Count)
        {
            return character.Colors[characterColor];
        }

        return DefaultColor;
    }
}
