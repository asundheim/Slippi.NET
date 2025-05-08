namespace Slippi.NET.Types;

public record class GameStart
{
    public GameStart(
        string? slpVersion, 
        TimerType? timerType, 
        int? inGameMode, 
        bool? friendlyFireEnabled, 
        bool? isTeams, 
        int? stageId, 
        uint? startingTimerSeconds, 
        ItemSpawnType? itemSpawnBehavior, 
        ulong? enabledItems, 
        List<Player> players, 
        byte? scene, 
        GameMode? gameMode, 
        Language? language, 
        GameInfo? gameInfoBlock, 
        uint? randomSeed, 
        bool? isPAL, 
        bool? isFrozenPS, 
        MatchInfo? matchInfo)
    {
        SlpVersion = slpVersion;
        TimerType = timerType;
        InGameMode = inGameMode;
        FriendlyFireEnabled = friendlyFireEnabled;
        IsTeams = isTeams;
        StageId = stageId;
        StartingTimerSeconds = startingTimerSeconds;
        ItemSpawnBehavior = itemSpawnBehavior;
        EnabledItems = enabledItems;
        Players = players;
        Scene = scene;
        GameMode = gameMode;
        Language = language;
        GameInfoBlock = gameInfoBlock;
        RandomSeed = randomSeed;
        IsPAL = isPAL;
        IsFrozenPS = isFrozenPS;
        MatchInfo = matchInfo;
    }

    public string? SlpVersion { get; set; }
    public TimerType? TimerType { get; set; }
    public int? InGameMode { get; set; }
    public bool? FriendlyFireEnabled { get; set; }
    public bool? IsTeams { get; set; }
    public int? StageId { get; set; }
    public uint? StartingTimerSeconds { get; set; }
    public ItemSpawnType? ItemSpawnBehavior { get; set; }
    public ulong? EnabledItems { get; set; }
    public List<Player> Players { get; set; }
    public byte? Scene { get; set; }
    public GameMode? GameMode { get; set; }
    public Language? Language { get; set; }
    public GameInfo? GameInfoBlock { get; set; }
    public uint? RandomSeed { get; set; }
    public bool? IsPAL { get; set; }
    public bool? IsFrozenPS { get; set; }
    public MatchInfo? MatchInfo { get; set; }
}