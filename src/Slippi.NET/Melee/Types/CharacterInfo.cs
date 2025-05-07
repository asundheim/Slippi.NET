using System.Diagnostics.CodeAnalysis;

namespace Slippi.NET.Melee.Types;

/// <summary>
/// Represents the data for a character.
/// </summary>
/// <param name="Name"></param>
/// <param name="ShortName"></param>
/// <param name="Colors"></param>
public record class CharacterInfo
{
    [SetsRequiredMembers]
    public CharacterInfo(int id, string name, string? shortName, List<string> colors)
    {
        Id = id;
        Name = name;
        ShortName = shortName;
        Colors = colors;
    }

    public CharacterInfo() { }

    public required int Id { get; init; }
    public required string Name { get; init; }
    public required string? ShortName { get; init; }
    public required List<string> Colors { get; init; }
}
