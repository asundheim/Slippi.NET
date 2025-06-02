namespace Slippi.NET.Types;

[Flags]
public enum StateFlags4 : byte
{
    Unknown_0x01 = 0x01,
    IsInHitStun = 0x02,
    OwnerHitboxTouchingShield = 0x04,
    Unknown_0x08 = 0x08,
    Unknown_0x10 = 0x10,
    PowershieldActive = 0x20,
    Unknown_0x40 = 0x40,
    Unknown_0x80 = 0x80
}
