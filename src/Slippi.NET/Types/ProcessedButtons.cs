namespace Slippi.NET.Types;

[Flags]
public enum ProcessedButtons : uint
{
    // match PhysicalButtons
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
    // end PhysicalButtons
    Stick_Up        = 0x10000,
    Stick_Down      = 0x20000,
    Stick_Left      = 0x40000,
    Stick_Right     = 0x80000,
    CStick_Up       = 0x100000,
    CStick_Down     = 0x200000,
    CStick_Left     = 0x400000,
    CStick_Right    = 0x800000,
    Unused_5        = 0x1000000,
    Unused_6        = 0x2000000,
    Unused_7        = 0x4000000,
    Unused_8        = 0x8000000,
    Unused_9        = 0x10000000,
    Unused_10       = 0x20000000,
    Unused_11       = 0x40000000,
    Any_Trigger     = 0x80000000,
}
