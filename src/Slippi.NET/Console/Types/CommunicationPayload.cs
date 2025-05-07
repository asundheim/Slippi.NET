using Newtonsoft.Json;

namespace Slippi.NET.Console.Types;

[JsonObject]
public class CommunicationPayload
{
    [JsonProperty(PropertyName = "cursor")]
    public required byte[]? Cursor { get; init; }

    [JsonProperty(PropertyName = "clientToken")]
    public required byte[]? ClientToken { get; init; }

    [JsonProperty(PropertyName = "pos")]
    public required byte[]? Pos { get; init; }

    [JsonProperty(PropertyName = "nextPos")]
    public required byte[]? NextPos { get; init; }

    [JsonProperty(PropertyName = "data")]
    public required byte[]? Data { get; init; }

    [JsonProperty(PropertyName = "nick")]
    public required string? Nick { get; init; }

    [JsonProperty(PropertyName = "forcePos")]
    public required bool? ForcePos { get; init; }

    [JsonProperty(PropertyName = "nintendontVersion")]
    public required string? NintendontVersion { get; init; }
}
