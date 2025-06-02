namespace Slippi.NET.Types;

[Flags]
public enum StateFlags2 : byte
{
    Unknown_0x01 = 0x01,
    Unknown_0x02 = 0x02,
    /// <summary>
    /// Has temporary intangibility or invincibility from subaction
    /// </summary>
    SubactionIntagibility = 0x04,
    IsFastFalling = 0x08,
    IsDefenderInHitLag = 0x10,
    IsInHitLag = 0x20,
    Unknown_0x40 = 0x40,
    Unknown_0x80 = 0x80
}
