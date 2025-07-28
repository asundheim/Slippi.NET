
namespace ComboRenderer.Interop.Types;

[Flags]
internal enum NotifyIconDataFlags : uint
{
    /// <summary>
    /// The <see cref="NOTIFYICONDATAW.uCallbackMessage"/> member is valid.
    /// </summary>
    NIF_MESSAGE = 0x1,
    /// <summary>
    /// The <see cref="NOTIFYICONDATAW.hIcon"/> member is valid.
    /// </summary>
    NIF_ICON = 0x2,
    /// <summary>
    /// The <see cref="NOTIFYICONDATAW.szTip"/> member is valid.
    /// </summary>
    NIF_TIP = 0x4,
    /// <summary>
    /// The <see cref="NOTIFYICONDATAW.dwState"/> and <see cref="NOTIFYICONDATAW.dwStateMask"/> members are valid.
    /// </summary>
    NIF_STATE = 0x8,
    /// <summary>
    /// Display a balloon notification. 
    /// The <see cref="NOTIFYICONDATAW.szInfo"/>, <see cref="NOTIFYICONDATAW.szInfoTitle"/>, 
    /// <see cref="NOTIFYICONDATAW.dwInfoFlags"/>, and <see cref="NOTIFYICONDATAW.uTimeoutOrVersion"/> members are valid. 
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>To display the balloon notification, specify <see cref="NIF_INFO"/> and provide text in <see cref="NOTIFYICONDATAW.szInfo"/>.</item>
    /// <item>To remove a balloon notification, specify <see cref="NIF_INFO"/> and provide an empty string through <see cref="NOTIFYICONDATAW.szInfo"/>.</item>
    /// <item>To add a notification area icon without displaying a notification, do not set the <see cref="NIF_INFO"/> flag.</item>
    /// </list>
    /// </remarks>
    NIF_INFO = 0x10,
    /// <summary>
    /// The <see cref="NOTIFYICONDATAW.guidItem"/> is valid.
    /// </summary>
    NIF_GUID = 0x20,
    /// <summary>
    /// If the balloon notification cannot be displayed immediately, discard it. 
    /// Use this flag for notifications that represent real-time information which would be meaningless or misleading if displayed at a later time. 
    /// For example, a message that states "Your telephone is ringing." <see cref="NIF_REALTIME"/> is meaningful only when combined with the <see cref="NIF_INFO"/> flag.
    /// </summary>
    NIF_REALTIME = 0x40,
    /// <summary>
    /// Use the standard tooltip. 
    /// Normally, when <see cref="NOTIFYICONDATAW.uTimeoutOrVersion"/> is set to NOTIFYICON_VERSION_4, 
    /// the standard tooltip is suppressed and can be replaced by the application-drawn, pop-up UI. 
    /// If the application wants to show the standard tooltip with NOTIFYICON_VERSION_4, 
    /// it can specify <see cref="NIF_SHOWTIP"/> to indicate the standard tooltip should still be shown.
    /// </summary>
    NIF_SHOWTIP = 0x80
}
