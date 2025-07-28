
namespace ComboRenderer.Interop.Types;

[Flags]
internal enum NotifyIconInfoFlags
{
    /// <summary>
    /// No icon.
    /// </summary>
    NIIF_NONE = 0x0,
    /// <summary>
    /// An information icon.
    /// </summary>
    NIIF_INFO = 0x1,
    /// <summary>
    /// A warning icon.
    /// </summary>
    NIIF_WARNING = 0x2,
    /// <summary>
    /// An error icon.
    /// </summary>
    NIIF_ERROR = 0x3,
    /// <summary>
    /// Use the icon identified in <see cref="NOTIFYICONDATAW.hBalloonIcon"/> as the notification balloon's title icon.
    /// </summary>
    NIIF_USER = 0x4,
    /// <summary>
    /// Do not play the associated sound. Applies only to notifications.
    /// </summary>
    NIIF_NOSOUND = 0x10,
    /// <summary>
    /// The large version of the icon should be used as the notification icon. 
    /// This corresponds to the icon with dimensions SM_CXICON x SM_CYICON. 
    /// If this flag is not set, the icon with dimensions SM_CXSMICON x SM_CYSMICON is used.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>This flag can be used with all stock icons.</item>
    /// <item>
    /// Applications that use older customized icons (NIIF_USER with hIcon) must provide a new SM_CXICON x SM_CYICON version in the tray icon (hIcon). 
    /// These icons are scaled down when they are displayed in the System Tray or System Control Area (SCA).
    /// </item>
    /// <item>New customized icons (NIIF_USER with hBalloonIcon) must supply an SM_CXICON x SM_CYICON version in the supplied icon (hBalloonIcon).</item>
    /// </list>
    /// </remarks>
    NIIF_LARGE_ICON = 0x20,
    /// <summary>
    /// Do not display the balloon notification if the current user is in "quiet time", which is the first hour after a new user logs into his or her account for the first time.
    /// During this time, most notifications should not be sent or shown. 
    /// This lets a user become accustomed to a new computer system without those distractions. 
    /// Quiet time also occurs for each user after an operating system upgrade or clean installation. 
    /// A notification sent with this flag during quiet time is not queued; it is simply dismissed unshown. 
    /// The application can resend the notification later if it is still valid at that time.
    /// If the current user is not in quiet time, this flag has no effect.
    /// </summary>
    NIIF_RESPECT_QUIET_TIME = 0x80,
    /// <summary>
    /// Reserved.
    /// </summary>
    NIIF_ICON_MASK = 0x0F
}