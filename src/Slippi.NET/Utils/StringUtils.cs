using System.Text;

namespace Slippi.NET.Utils;

internal static class StringUtils
{
    static StringUtils()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public static Encoding Shift_JIS { get; } = Encoding.GetEncoding("Shift-JIS");

    public static string? ReadShiftJIS(Span<byte> buffer)
    {
        return Shift_JIS.GetString(buffer);
    }

    public static string? ReadUtf8(Span<byte> buffer)
    {
        return Encoding.UTF8.GetString(buffer);
    }
}
