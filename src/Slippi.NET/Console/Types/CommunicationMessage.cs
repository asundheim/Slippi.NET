using Newtonsoft.Json;

namespace Slippi.NET.Console.Types;

[JsonObject]
public class CommunicationMessage
{
    [JsonProperty(PropertyName = "type")]
    public required CommunicationType Type { get; init; }

    [JsonProperty(PropertyName = "payload")]
    public required CommunicationPayload Payload { get; init; }
}