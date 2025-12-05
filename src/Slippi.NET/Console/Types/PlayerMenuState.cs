using Slippi.NET.Melee.Types;

namespace Slippi.NET.Console.Types;

public record PlayerMenuState
{
    public required int PlayerIndex { get; init; }
    public required MenuControllerStatus ControllerStatus { get; init; }
    public float? CursorX { get; set; }
    public float? CursorY { get; set; }
    public Character? Character { get; set; }
    public byte? CharacterColor { get; set; }
    public bool CoinDown { get; set; } = false;

    public bool Is(PlayerMenuState? other)
    {
        if (other is null) return false;

        if (ReferenceEquals(this, other)) return true;

        return PlayerIndex == other.PlayerIndex &&
               ControllerStatus == other.ControllerStatus &&
               CursorX == other.CursorX &&
               CursorY == other.CursorY &&
               Character == other.Character &&
               CharacterColor == other.CharacterColor &&
               CoinDown == other.CoinDown;
    }
}
