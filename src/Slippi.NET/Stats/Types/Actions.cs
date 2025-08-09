using Slippi.NET.Types;

namespace Slippi.NET.Stats.Types;

/// <summary>
/// A synthesized action based on <see cref="ActionState"/>s.
/// </summary>
public enum Actions
{
    None,
    Wavedash,
    Waveland,
    AirDodge,
    DashDance,
    SpotDodge,
    Ledgegrab,
    Roll,
    LCancel,
    Jab,
    DashAttack,
    FTilt,
    UTilt,
    DTilt,
    FSmash,
    USmash,
    DSmash,
    Nair,
    Fair,
    Bair,
    UAir,
    DAir,
    Grab,
    UThrow,
    DThrow,
    FThrow,
    BThrow,
    Tech,
    Jump,
    Dash,
    Shine,
    ShineEnd,           // fox or falco
    ShineTurnaround,    // fox or falco
    Laser,              // fox or falco
    JumpCancel,
    SideB,
    FirefoxStartup,     // fox
    Firefox,            // fox
    ShieldStart,
    Shield,
    PlatformDrop,
    WallJump,
    ShieldDrop,
    FastFall,
    FalcoSideB,         // falco
    FireBirdStartup,    // falco
    FireBird,           // falco
}
