namespace ComboInterpreter;

[Flags]
public enum SimpleButtons
{
    NONE            = 0x0,
    STICK_DOWN      = 0x1,
    STICK_UP        = 0x2,
    STICK_LEFT      = 0x4,
    STICK_RIGHT     = 0x8,
    CSTICK_DOWN     = 0x10,
    CSTICK_UP       = 0x20,
    CSTICK_LEFT     = 0x40,
    CSTICK_RIGHT    = 0x80,
    A               = 0x100,
    B               = 0x200,
    X               = 0x400,
    Y               = 0x800,
    Z               = 0x1000,
    RT               = 0x2000,
    LT               = 0x4000,
    // ...
    // X_Y          = X | Y,
    // L_R          = L | R,
}
