using Newtonsoft.Json;

namespace Slippi.NET.Console.Types;

[JsonObject]
public class DolphinMessage
{
    [JsonProperty(PropertyName = "type")]
    public required string Type { get; set; }

    [JsonProperty(PropertyName = "cursor")]
    public int? GameCursor { get; set; }

    [JsonProperty(PropertyName = "next_cursor")]
    public int? NextCursor { get; set; }

    [JsonProperty(PropertyName = "payload")]
    public string? Payload { get; set; }

    [JsonProperty(PropertyName = "nick")]
    public string? Nickname { get; set; }

    [JsonProperty(PropertyName = "version")]
    public string? Version { get; set; }

    [JsonProperty(PropertyName = "dolphin_closed")]
    public bool? DolphinClosed { get; set; }
}
