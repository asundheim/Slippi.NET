using Newtonsoft.Json;
using System.Globalization;
using Slippi.NET.Melee;
using Slippi.NET.Melee.Types;

namespace Slippi.NET.Types;

[JsonObject]
public class Metadata
{
    /// <summary>
    /// The start date of the game.
    /// </summary>
    /// <remarks>
    /// Formatted as ISO-8601. Use <see cref="GetStartDate"/> as a convenience for parsing this manually.
    /// </remarks>
    [JsonProperty(PropertyName = "startAt")]
    public string? StartAt { get; set; }

    /// <summary>
    /// The platform this game was played on, e.g. "dolphin"
    /// </summary>
    [JsonProperty(PropertyName = "playedOn")]
    public string? PlayedOn { get; set; }

    /// <summary>
    /// The number of the last frame of this game.
    /// </summary>
    [JsonProperty(PropertyName = "lastFrame")]
    public int? LastFrame { get; set; }

    /// <summary>
    /// Dictionary mapping player index (0-4) to <see cref="PlayerMetadata"/>
    /// </summary>
    [JsonProperty(PropertyName = "players")]
    public required Dictionary<int, PlayerMetadata> Players { get; set; }

    /// <summary>
    /// The nickname of the console where the game was played.
    /// </summary>
    [JsonProperty(PropertyName = "consoleNick")]
    public string? ConsoleNick { get; set; }

    /// <summary>
    /// Get the start date of this game as a <see cref="DateTime"/> object.
    /// </summary>
    /// <remarks>
    /// Null if no start date was found.
    /// </remarks>
    public DateTime? GetStartDate()
    {
        return string.IsNullOrEmpty(StartAt) ? null : DateTime.Parse(StartAt, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.RoundtripKind);
    }
}

/// <summary>
/// Player information metadata.
/// </summary>
[JsonObject]
public class PlayerMetadata
{
    /// <summary>
    /// Dictionary mapping character id (<see cref="Character"/>) to the number of frames that character was played for.
    /// </summary>
    /// <remarks>
    /// To get full character information, use <see cref="CharacterUtils.GetCharacterInfo(int)"/>
    /// </remarks>
    [JsonProperty(PropertyName = "characters")]
    public required Dictionary<Character, int> CharacterUsage { get; set; }

    /// <summary>
    /// Player names. May not be present for non-netplay games.
    /// </summary>
    [JsonProperty(PropertyName = "names")]
    public required PlayerNameInfo? Names { get; set; }
}

[JsonObject]
public class PlayerNameInfo
{
    /// <summary>
    /// Netplay username.
    /// </summary>
    /// <remarks>
    /// May not be present for non-netplay games.
    /// </remarks>
    [JsonProperty(PropertyName = "netplay")]
    public required string? Netplay { get; set; }

    /// <summary>
    /// Netplay connect code, e.g. D#345
    /// </summary>
    /// <remarks>
    /// May not be present for non-netplay games.
    /// </remarks>
    [JsonProperty(PropertyName = "code")]
    public string? Code { get; set; }
}