namespace Slippi.NET.Types;

[Flags]
public enum StateFlags1 : byte
{
    Unknown_0x01 = 0x01,
    /// <summary>
    /// e.g. GnW bucket
    /// </summary>
    AbsorberActive = 0x02,
    Unknown_0x04 = 0x04,
    /// <summary>
    /// Active when reflect does not change projectile ownership (mewtwo side b)
    /// </summary>
    ProjectileReflectOwnerUnchanged = 0x08,
    IsReflectActive = 0x10,
    Unknown_0x20 = 0x20,
    Unknown_0x40 = 0x40,
    Unknown_0x80 = 0x80
}
