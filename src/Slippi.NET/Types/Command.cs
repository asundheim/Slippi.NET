namespace Slippi.NET.Types;

public enum Command
{
    SPLIT_MESSAGE = 0x10,
    MESSAGE_SIZES = 0x35,
    GAME_START = 0x36,
    PRE_FRAME_UPDATE = 0x37,
    POST_FRAME_UPDATE = 0x38,
    GAME_END = 0x39,
    FRAME_START = 0x3a,
    ITEM_UPDATE = 0x3b,
    FRAME_BOOKEND = 0x3c,
    GECKO_LIST = 0x3d
}