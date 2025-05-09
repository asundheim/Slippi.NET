using System.Diagnostics;

namespace Slippi.NET.Types;

public record class Player
{
    public Player(int playerIndex, 
        int port, 
        int? characterId, 
        int? type, 
        int? startStocks, 
        int? characterColor, 
        int? teamShade, 
        int? handicap, 
        int? teamId, 
        bool? staminaMode, 
        bool? silentCharacter, 
        bool? invisible, 
        bool? lowGravity, 
        bool? blackStockIcon,
        bool? metal, 
        bool? startOnAngelPlatform,
        bool? rumbleEnabled, 
        int? cpuLevel, 
        float? offenseRatio, 
        float? defenseRatio,
        float? modelScale,
        string? controllerFix, 
        string? nametag, 
        string? displayName, 
        string? connectCode, 
        string? userId)
    {
        PlayerIndex = playerIndex;
        Port = port;
        CharacterId = characterId;
        Type = type;
        StartStocks = startStocks;
        CharacterColor = characterColor;
        TeamShade = teamShade;
        Handicap = handicap;
        TeamId = teamId;
        StaminaMode = staminaMode;
        SilentCharacter = silentCharacter;
        Invisible = invisible;
        LowGravity = lowGravity;
        BlackStockIcon = blackStockIcon;
        Metal = metal;
        StartOnAngelPlatform = startOnAngelPlatform;
        RumbleEnabled = rumbleEnabled;
        CpuLevel = cpuLevel;
        OffenseRatio = offenseRatio;
        DefenseRatio = defenseRatio;
        ModelScale = modelScale;
        ControllerFix = controllerFix;
        Nametag = nametag;
        DisplayName = displayName;
        ConnectCode = connectCode;
        UserId = userId;
    }

    public int PlayerIndex { get; set; }
    public int Port { get; set; }

    [DebuggerDisplay("{CharacterId} ({CharacterId.HasValue ? Slippi.NET.Melee.CharacterUtils.GetCharacterName(CharacterId.Value) : null})")]
    public int? CharacterId { get; set; }
    public int? Type { get; set; }
    public int? StartStocks { get; set; }
    public int? CharacterColor { get; set; }
    public int? TeamShade { get; set; }
    public int? Handicap { get; set; }
    public int? TeamId { get; set; }
    public bool? StaminaMode { get; set; }
    public bool? SilentCharacter { get; set; }
    public bool? Invisible { get; set; }
    public bool? LowGravity { get; set; }
    public bool? BlackStockIcon { get; set; }
    public bool? Metal { get; set; }
    public bool? StartOnAngelPlatform { get; set; }
    public bool? RumbleEnabled { get; set; }
    public int? CpuLevel { get; set; }
    public float? OffenseRatio { get; set; }
    public float? DefenseRatio { get; set; }
    public float? ModelScale { get; set; }
    public string? ControllerFix { get; set; }
    public string? Nametag { get; set; }
    public string? DisplayName { get; set; }
    public string? ConnectCode { get; set; }
    public string? UserId { get; set; }
}
