
namespace ComboRenderer.Interop.Types;

[Flags]
internal enum NotifyIconStateFlags : uint
{
    /// <summary>
    /// The icon is hidden.
    /// </summary>
    NIS_HIDDEN = 0x1,
    /// <summary>
    /// The icon resource is shared between multiple icons.
    /// </summary>
    NIS_SHAREDICON = 0x2
}
