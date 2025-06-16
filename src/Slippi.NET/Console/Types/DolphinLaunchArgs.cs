using Newtonsoft.Json;
using Slippi.NET.Types;

namespace Slippi.NET.Console.Types;

[JsonObject]
public class DolphinLaunchArgs
{
    /// <summary>
    /// The path to the replay if using <see cref="DolphinLaunchModes.Normal"/> or <see cref="DolphinLaunchModes.Mirror"/>.
    /// </summary>
    [JsonProperty(PropertyName = "replay")]
    public string? Replay { get; set; } = null;

    /// <summary>
    /// Possible values are contained in <see cref="DolphinLaunchModes"/>.
    /// </summary>
    [JsonProperty(PropertyName = "mode")]
    public string Mode { get; set; } = DolphinLaunchModes.Normal;

    /// <summary>
    /// The frame you would like to start the replay on, default is <see cref="Frames.FIRST"/>.
    /// </summary>
    [JsonProperty(PropertyName = "startFrame")]
    public int? StartFrame { get; set; } = null;

    /// <summary>
    /// The frame you would like to end the replay on, default is <see cref="int.MaxValue"/>.
    /// </summary>
    [JsonProperty(PropertyName = "endFrame")]
    public int? EndFrame { get; set; } = null;

    /// <summary>
    /// Will output the console name and time of replay to the Slippi folder next to the Dolphin executable (this only works when using queue mode), 
    /// default is <see langword="false"/>.
    /// </summary>
    [JsonProperty(PropertyName = "outputOverlayFiles")]
    public bool OutputOverlayFiles { get; set; } = false;

    /// <summary>
    /// Typically used to indicate that the replay has changed, but updating the value can also restart playback of the current replay or queue
    /// </summary>
    /// <remarks>
    /// Unsure how this is supposed to work, especially given the sparse docs.
    /// </remarks>
    [JsonProperty(PropertyName = "commandId")]
    public string CommandId { get; set; } = string.Empty;

    /// <summary>
    /// Will force dolphin to stay closer to realtime which is important for mirroring, default is <see langword="false"/>.
    /// </summary>
    [JsonProperty(PropertyName = "isRealTimeMode")]
    public bool IsRealTimeMode { get; set; } = false;

    /// <summary>
    /// Indicates whether the resync logic should be used. Resync logic will allow playback to go back to normal after a desync, 
    /// default is <see langword="true"/>.
    /// </summary>
    [JsonProperty(PropertyName = "shouldResync")]
    public bool ShouldResync { get; set; } = true;

    /// <summary>
    /// Tells dolphin to display rollbacks either like the player saw them (normal) 
    /// or by showing every frame in the file (visible). 
    /// Possible values are <see cref="RollbackDisplayMethods.Off"/> (default), 
    /// <see cref="RollbackDisplayMethods.Normal"/>, and <see cref="RollbackDisplayMethods.Visible"/>.
    /// </summary>
    [JsonProperty(PropertyName = "rollbackDisplayMethod")]
    public string RollbackDisplayMethod { get; set; } = RollbackDisplayMethods.Off;

    /// <summary>
    /// Typically the name of console or broadcaster, used in the Dolphin window title to uniquely identify Dolphin instances.
    /// </summary>
    [JsonProperty(PropertyName = "gameStation")]
    public string GameStation { get; set; } = string.Empty;

    /// <summary>
    /// All files in the queue will be played back to back. 
    /// This is commonly used for set recordings or combo video recordings.
    /// </summary>
    [JsonProperty(PropertyName = "queue")]
    public IList<QueueItem> Queue { get; set; } = [];
}

[JsonObject]
public class QueueItem
{
    /// <summary>
    /// The path to the replay.
    /// </summary>
    [JsonProperty(PropertyName = "path")]
    public required string Path { get; set; }

    /// <summary>
    /// The frame you would like to start the replay on, default is <see cref="Frames.FIRST"/>.
    /// </summary>
    [JsonProperty(PropertyName = "startFrame")]
    public int StartFrame { get; set; } = (int)Frames.FIRST;

    /// <summary>
    /// The frame you would like to end the replay on, default is <see cref="int.MaxValue"/>.
    /// </summary>
    [JsonProperty(PropertyName = "endFrame")]
    public int EndFrame { get; set; } = int.MaxValue;

    /// <summary>
    /// Typically the name of console the replay was created on, but can be used with any string
    /// </summary>
    [JsonProperty(PropertyName = "gameStartAt")]
    public string GameStartAt { get; set; } = string.Empty;

    /// <summary>
    /// Typically the name of console the replay was created on, but can be used with any string.
    /// </summary>
    [JsonProperty(PropertyName = "gameStation")]
    public string GameStation { get; set; } = string.Empty;
}
