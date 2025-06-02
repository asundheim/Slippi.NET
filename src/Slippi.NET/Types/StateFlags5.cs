namespace Slippi.NET.Types;

[Flags]
public enum StateFlags5 : byte
{
    Unknown_0x01 = 0x01,
    IsCloakingDevice = 0x02,
    Unknown_0x04 = 0x04,
    IsFollower = 0x08,
    /// <summary>
    /// Is inactive (zelda/shiek when opposite is in play, 0 stock teammate, etc.) Bit should always be 0 in replays.
    /// </summary>
    IsInactive = 0x10,
    Unknown_0x20 = 0x20,
    IsDead = 0x40,
    IsOffScreen = 0x80
}
