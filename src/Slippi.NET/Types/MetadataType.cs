namespace Slippi.NET.Types;

public record class MetadataType
{
    public MetadataType(string? startAt, string? playedOn, int? lastFrame, Dictionary<int, PlayerMetadata>? players, string? consoleNick)
    {
        StartAt = startAt;
        PlayedOn = playedOn;
        LastFrame = lastFrame;
        Players = players;
        ConsoleNick = consoleNick;
    }

    public string? StartAt { get; set; }
    public string? PlayedOn { get; set; }
    public int? LastFrame { get; set; }
    public Dictionary<int, PlayerMetadata>? Players { get; set; }
    public string? ConsoleNick { get; set; }
}

public record class PlayerMetadata
{
    public PlayerMetadata(Dictionary<int, int> characters, PlayerNames? names)
    {
        Characters = characters;
        Names = names;
    }

    public Dictionary<int, int> Characters { get; set; } = new();
    public PlayerNames? Names { get; set; }
}

public record class PlayerNames
{
    public PlayerNames(string? netplay, string? code)
    {
        Netplay = netplay;
        Code = code;
    }

    public string? Netplay { get; set; }
    public string? Code { get; set; }
}