namespace Slippi.NET.Types;

public record class GameStartType
{
    public GameStartType(
        string? slpVersion, 
        TimerType? timerType, 
        int? inGameMode, 
        bool? friendlyFireEnabled, 
        bool? isTeams, 
        int? stageId, 
        int? startingTimerSeconds, 
        ItemSpawnType? itemSpawnBehavior, 
        int? enabledItems, 
        List<PlayerType> players, 
        int? scene, 
        GameMode? gameMode, 
        Language? language, 
        GameInfoType? gameInfoBlock, 
        int? randomSeed, 
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
    public int? StartingTimerSeconds { get; set; }
    public ItemSpawnType? ItemSpawnBehavior { get; set; }
    public int? EnabledItems { get; set; }
    public List<PlayerType> Players { get; set; }
    public int? Scene { get; set; }
    public GameMode? GameMode { get; set; }
    public Language? Language { get; set; }
    public GameInfoType? GameInfoBlock { get; set; }
    public int? RandomSeed { get; set; }
    public bool? IsPAL { get; set; }
    public bool? IsFrozenPS { get; set; }
    public MatchInfo? MatchInfo { get; set; }
}