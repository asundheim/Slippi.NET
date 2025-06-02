namespace Slippi.NET.Types;

[Flags]
public enum StateFlags3 : byte
{
    Unknown_0x01 = 0x01,
    Unknown_0x02 = 0x02,
    GrabbingOtherCharacter = 0x04,
    Unknown_0x08 = 0x08,
    Unknown_0x10 = 0x10,
    Unknown_0x20 = 0x20,
    Unknown_0x40 = 0x40,
    IsShieldActive = 0x80
}
