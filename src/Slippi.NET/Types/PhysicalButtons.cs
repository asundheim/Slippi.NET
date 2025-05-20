namespace Slippi.NET.Types;

[Flags]
public enum PhysicalButtons : ushort
{
    DPadLeft        = 0x1,
    DPadRight       = 0x2,
    DPadDown        = 0x4,
    DPadUp          = 0x8,
    Z               = 0x10,
    RT              = 0x20,
    LT              = 0x40,
    Unused_1        = 0x80,
    A               = 0x100,
    B               = 0x200,
    X               = 0x400,
    Y               = 0x800,
    Start           = 0x1000,
    Unused_2        = 0x2000,
    Unused_3        = 0x4000,
    Unused_4        = 0x8000,
}
