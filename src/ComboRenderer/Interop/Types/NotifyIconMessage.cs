
namespace ComboRenderer.Interop.Types;

internal enum NotifyIconMessage : uint
{
    /// <summary>
    /// Adds an icon to the status area. 
    /// The icon is given an identifier in the <see cref="NOTIFYICONDATAW"/> structure pointed to by lpdata — 
    /// either through its <see cref="NOTIFYICONDATAW.uID"/> or <see cref="NOTIFYICONDATAW.guidItem"/> member. 
    /// This identifier is used in subsequent calls to Shell_NotifyIcon to perform later actions on the icon.
    /// </summary>
    NIM_ADD = 0x0,
    /// <summary>
    /// Modifies an icon in the status area. 
    /// The <see cref="NOTIFYICONDATAW"/> structure pointed to by lpdata uses the ID 
    /// originally assigned to the icon when it was added to the notification area (<see cref="NIM_ADD"/>) to identify the icon to be modified.
    /// </summary>
    NIM_MODIFY = 0x1,
    /// <summary>
    /// Deletes an icon from the status area. 
    /// The <see cref="NOTIFYICONDATAW"/> structure pointed to by lpdata uses the ID 
    /// originally assigned to the icon when it was added to the notification area (<see cref="NIM_ADD"/>) to identify the icon to be deleted.
    /// </summary>
    NIM_DELETE = 0x2,
    /// <summary>
    /// Returns focus to the taskbar notification area. 
    /// Notification area icons should use this message when they have completed their UI operation. 
    /// For example, if the icon displays a shortcut menu, but the user presses ESC to cancel it, use <see cref="NIM_SETFOCUS"/>to return focus to the notification area.
    /// </summary>
    NIM_SETFOCUS = 0x3,
    /// <summary>
    /// Instructs the notification area to behave according to the version number specified in the uVersion member of the structure pointed to by lpdata. 
    /// The version number specifies which members are recognized.
    /// </summary>
    NIM_SETVERSION = 0x4
}
