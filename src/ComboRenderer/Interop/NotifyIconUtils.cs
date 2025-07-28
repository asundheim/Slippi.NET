using ComboRenderer.Interop.Types;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace ComboRenderer;

internal static partial class NotifyIconUtils
{
    [LibraryImport("Shell32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static unsafe partial bool Shell_NotifyIconW(NotifyIconMessage dwMessage, NOTIFYICONDATAW* lpData);
}

// I couldn't get a WPF ContextMenu to behave and close itself but this does work as a primitive wrapper over Win32 Shell_NotifyIcon
// Edit: it turns out you just have to call Win32 SetForegroundWindow on the ContextMenu hwnd and not Win32 SetFocus
internal class NotifyIcon : IDisposable
{
    private readonly Guid _guidId;
    private readonly DestroyIconSafeHandle _iconHandle;

    public const int MESSAGE_ID = 0x4242;

    /// <param name="icon">A handle from <see cref="PInvoke.CreateIconFromResourceEx"/></param>
    public unsafe NotifyIcon(HWND owner, DestroyIconSafeHandle icon)
    {
        _guidId = Guid.NewGuid();
        _iconHandle = icon;

        NOTIFYICONDATAW addIcon = new NOTIFYICONDATAW()
        {
            cbSize = sizeof(NOTIFYICONDATAW),
            hWnd = owner,
            uID = 0,
            uFlags = NotifyIconDataFlags.NIF_ICON | 
                     NotifyIconDataFlags.NIF_GUID | 
                     NotifyIconDataFlags.NIF_MESSAGE,
            uCallbackMessage = MESSAGE_ID,
            hIcon = new HICON(icon.DangerousGetHandle()),
            guidItem = _guidId
        };
        NotifyIconUtils.Shell_NotifyIconW(NotifyIconMessage.NIM_ADD, (NOTIFYICONDATAW*)Unsafe.AsPointer(ref addIcon));

        NOTIFYICONDATAW versionIcon = new NOTIFYICONDATAW()
        {
            cbSize = sizeof(NOTIFYICONDATAW),
            uFlags = NotifyIconDataFlags.NIF_GUID,
            uTimeoutOrVersion = PInvoke.NOTIFYICON_VERSION_4,
            guidItem = _guidId
        };
        NotifyIconUtils.Shell_NotifyIconW(NotifyIconMessage.NIM_SETVERSION, (NOTIFYICONDATAW*)Unsafe.AsPointer(ref versionIcon));
    }

    public unsafe void Dispose()
    {
        NOTIFYICONDATAW closeIcon = new NOTIFYICONDATAW()
        {
            cbSize = sizeof(NOTIFYICONDATAW),
            uFlags = NotifyIconDataFlags.NIF_GUID,
            guidItem = _guidId
        };
        NotifyIconUtils.Shell_NotifyIconW(NotifyIconMessage.NIM_DELETE, (NOTIFYICONDATAW*)Unsafe.AsPointer(ref closeIcon));

        _iconHandle.Dispose();
    }
}
