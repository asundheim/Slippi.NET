using Slippi.NET.Melee.Types;

namespace Slippi.NET.Console.Types;

public record MenuEvent
{
    public required MenuScene Menu { get; init; }
    public required SubMenuScene SubMenu { get; init; }
    public MenuOnlineMode? OnlineMode { get; set; }
    public required int FrameCount { get; init; }
    public required List<PlayerMenuState>? PlayerStates { get; init; }
    public Stage? Stage { get; set; }
    public float? StageSelectX { get; set; }
    public float? StageSelectY { get; set; }
    public bool ReadyToStart { get; set; } = false;
    public required byte[] RawEvent { get; init; }

    public bool Is(MenuEvent? other)
    {
        if (other is null) return false;

        if (ReferenceEquals(this, other)) return true;

        if (Menu != other.Menu ||
            SubMenu != other.SubMenu ||
            FrameCount != other.FrameCount ||
            ReadyToStart != other.ReadyToStart)
        {
            return false;
        }

        if ((PlayerStates is null) != (other.PlayerStates is null))
        {
            return false;
        }

        if (PlayerStates is not null && other.PlayerStates is not null)
        {
            if (PlayerStates.Count != other.PlayerStates.Count)
            {
                return false;
            }

            for (int i = 0; i < PlayerStates.Count; i++)
            {
                if (!PlayerStates[i].Is(other.PlayerStates[i]))
                {
                    return false;
                }
            }
        }

        return Stage == other.Stage &&
               StageSelectX == other.StageSelectX &&
               StageSelectY == other.StageSelectY;
    }
}
