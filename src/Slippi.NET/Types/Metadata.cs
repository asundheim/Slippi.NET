using Newtonsoft.Json;

namespace Slippi.NET.Types;

[JsonObject]
public class Metadata
{
    public Metadata(string? startAt, string? playedOn, int? lastFrame, Dictionary<int, PlayerMetadata>? players, string? consoleNick)
    {
        StartAt = startAt;
        PlayedOn = playedOn;
        LastFrame = lastFrame;
        Players = players;
        ConsoleNick = consoleNick;
    }

    [JsonProperty(PropertyName = "startAt")]
    public string? StartAt { get; set; }

    [JsonProperty(PropertyName = "playedOn")]
    public string? PlayedOn { get; set; }

    [JsonProperty(PropertyName = "lastFrame")]
    public int? LastFrame { get; set; }

    [JsonProperty(PropertyName = "players")]
    public Dictionary<int, PlayerMetadata>? Players { get; set; }

    [JsonProperty(PropertyName = "consoleNick")]
    public string? ConsoleNick { get; set; }
}

[JsonObject]
public class PlayerMetadata
{
    [JsonProperty(PropertyName = "characters")]
    public required Dictionary<int, int> Characters { get; set; }

    [JsonProperty(PropertyName = "names")]
    public required PlayerNames? Names { get; set; }
}

[JsonObject]
public class PlayerNames
{
    [JsonProperty(PropertyName = "netplay")]
    public required string? Netplay { get; set; }

    [JsonProperty(PropertyName = "code")]
    public string? Code { get; set; }
}