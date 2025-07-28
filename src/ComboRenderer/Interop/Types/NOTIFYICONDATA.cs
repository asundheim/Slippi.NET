using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace ComboRenderer.Interop.Types;

// amen
internal unsafe ref struct NOTIFYICONDATAW
{
    /// <summary>
    /// The size of this structure, in bytes.
    /// </summary>
    public int cbSize;
    /// <summary>
    /// A handle to the window that receives notifications associated with an icon in the notification area.
    /// </summary>
    public HWND hWnd;
    /// <summary>
    /// The application-defined identifier of the taskbar icon. 
    /// The Shell uses either (hWnd plus uID) or guidItem to identify which icon to operate on when Shell_NotifyIcon is invoked.
    /// If guidItem is specified, uID is ignored.
    /// </summary>
    public uint uID;
    /// <summary>
    /// Flags that either indicate which of the other members of the structure contain valid data or provide additional information to the tooltip as to how it should display.
    /// </summary>
    public NotifyIconDataFlags uFlags;
    /// <summary>
    /// An application-defined message identifier. The system uses this identifier to send notification messages to the window identified in hWnd. 
    /// </summary>
    public uint uCallbackMessage;
    /// <summary>
    /// A handle to the icon to be added, modified, or deleted.
    /// </summary>
    /// <remarks>
    /// If only a 16x16 pixel icon is provided, it is scaled to a larger size in a system set to a high dpi value.
    /// </remarks>
    public HICON hIcon;
    /// <summary>
    /// A null-terminated string that specifies the text for a standard tooltip. It can have a maximum of 64 characters, including the terminating null character.
    /// </summary>
    public fixed char szTip[128];
    /// <summary>
    /// The state of the icon. Flags.
    /// </summary>
    public NotifyIconStateFlags dwState;
    /// <summary>
    /// A value that specifies which bits of the <see cref="dwState"/> member are retrieved or modified. 
    /// The possible values are the same as those for dwState. 
    /// For example, setting this member to <see cref="NotifyIconStateFlags.NIS_HIDDEN"/> causes 
    /// only the item's hidden state to be modified while the icon sharing bit is ignored regardless of its value.
    /// </summary>
    public uint dwStateMask;
    /// <summary>
    /// A null-terminated string that specifies the text to display in a balloon notification. 
    /// It can have a maximum of 256 characters, including the terminating null character, but should be restricted to 200 characters in English to accommodate localization. 
    /// To remove the balloon notification from the UI, either delete the icon (with <see cref="NotifyIconUtils.NotifyIconMessage.NIM_DELETE"/>) or set the NIF_INFO flag in uFlags and set szInfo to an empty string.
    /// </summary>
    public fixed short szInfo[256];
    /// <summary>
    /// The timeout value, in milliseconds, for notification.
    /// </summary>
    public uint uTimeoutOrVersion;
    /// <summary>
    /// A null-terminated string that specifies a title for a balloon notification. 
    /// This title appears in a larger font immediately above the text. 
    /// It can have a maximum of 64 characters, including the terminating null character, but should be restricted to 48 characters in English to accommodate localization.
    /// </summary>
    public fixed short szInfoTitle[64];
    /// <summary>
    /// Flags that can be set to modify the behavior and appearance of a balloon notification. 
    /// The icon is placed to the left of the title. If the <see cref="szInfoTitle"/> member is zero-length, the icon is not shown.
    /// </summary>
    public NotifyIconInfoFlags dwInfoFlags;
    /// <summary>
    /// A registered GUID that identifies the icon. 
    /// This value overrides uID and is the recommended method of identifying the icon. 
    /// The <see cref="NotifyIconDataFlags.NIF_GUID"/> flag must be set in the <see cref="uFlags"/> member.
    /// </summary>
    public Guid guidItem;
    /// <summary>
    /// The handle of a customized notification icon provided by the application that should be used independently of the notification area icon. 
    /// If this member is non-NULL and the <see cref="NotifyIconInfoFlags.NIIF_USER"/> flag is set in the <see cref="dwInfoFlags"/> member, 
    /// this icon is used as the notification icon. 
    /// If this member is NULL, the legacy behavior is carried out.
    /// </summary>
    public HICON hBalloonIcon;
}
