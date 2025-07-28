using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Accessibility;
using static Windows.Win32.PInvoke;

namespace ComboRenderer.Interop;

internal class WindowInfo
{
    public int Left { get; set; }
    public int Top { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public HWND HWND { get; set; }
}

internal partial class DolphinWindowTracker : IDisposable
{
    private bool _isPlaybackDolphin;
    private WindowInfo? _dolphinInfo = null;

    private UnhookWinEventSafeHandle? _eventHook = null;
    private GCHandle? _callbackHandle = null;

    [GeneratedRegex(@"Faster Melee - Slippi \(.*\) - Playback", RegexOptions.Compiled)]
    private static partial Regex _playbackRegex();

    [GeneratedRegex(@"Faster Melee - Slippi \(.*\)", RegexOptions.Compiled)]
    private static partial Regex _liveRegex();

    [GeneratedRegex(@"Dolphin")]
    private static partial Regex _liveRegex2();

    public DolphinWindowTracker(bool isPlaybackDolphin = true) 
    { 
        _isPlaybackDolphin = isPlaybackDolphin;
    }

    public event EventHandler? OnDolphinMoved;

    [SupportedOSPlatform("windows5.0")]
    public unsafe WindowInfo? GetDolphinWindowInfo(bool hookEvents = true)
    {
        bool result = PInvoke.EnumWindows((HWND hwnd, LPARAM _) =>
        {
            Span<char> pWindowTextBuffer = stackalloc char[500];
            int windowNameLength = PInvoke.GetWindowText(hwnd, pWindowTextBuffer);

            string windowName = new string(pWindowTextBuffer.Slice(0, windowNameLength));
            Match m = (_isPlaybackDolphin ? _playbackRegex() : _liveRegex()).Match(windowName);
            if (m.Success || (!_isPlaybackDolphin && (_liveRegex2().Match(windowName)).Success))
            {
                if (PInvoke.GetWindowRect(hwnd, out RECT rect))
                {
                    _dolphinInfo = new WindowInfo
                    {
                        Left = rect.left,
                        Top = rect.top,
                        Width = rect.right - rect.left,
                        Height = rect.bottom - rect.top,
                        HWND = hwnd
                    };

                    return false;
                }
            }

            return true;
        }, IntPtr.Zero);

        if (!result && hookEvents && _dolphinInfo is not null)
        {
            uint pid = 0;
            uint hr = PInvoke.GetWindowThreadProcessId(_dolphinInfo.HWND, &pid);
            WINEVENTPROC OnMoved = (HWINEVENTHOOK _, uint _, HWND _, int _, int _, uint _, uint _) =>
            {
                OnDolphinMoved?.Invoke(null, EventArgs.Empty);
            };

            _callbackHandle = GCHandle.Alloc(OnMoved);

            _eventHook = PInvoke.SetWinEventHook(EVENT_SYSTEM_MOVESIZESTART, EVENT_SYSTEM_MOVESIZEEND, null, OnMoved, pid, 0, WINEVENT_OUTOFCONTEXT | WINEVENT_SKIPOWNPROCESS);
        }

        return result ? null : _dolphinInfo;
    }

    public unsafe WindowInfo? GetMovedDolphinWindowInfo()
    {
        if (_dolphinInfo is not null)
        {
            if (PInvoke.GetWindowRect(_dolphinInfo.HWND, out RECT rect))
            {
                return new WindowInfo
                {
                    Left = rect.left,
                    Top = rect.top,
                    Width = rect.right - rect.left,
                    Height = rect.bottom - rect.top,
                    HWND = _dolphinInfo.HWND
                };
            }
        }

        return null;
    }

    public void Dispose()
    {
        _callbackHandle?.Free();
        _eventHook?.Close();
    }
}
