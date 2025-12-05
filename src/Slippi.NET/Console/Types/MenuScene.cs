namespace Slippi.NET.Console.Types;

public enum MenuScene : ushort
{
    CHARACTER_SELECT = 0x0,
    STAGE_SELECT = 0x1,
    IN_GAME = 0x2,
    SUDDEN_DEATH = 0x3,
    POSTGAME_SCORES = 0x4,
    MAIN_MENU = 0x5,
    SLIPPI_ONLINE_CSS = 0x6,
    PRESS_START = 0x7,
    UNKNOWN_MENU = 0xff
}
