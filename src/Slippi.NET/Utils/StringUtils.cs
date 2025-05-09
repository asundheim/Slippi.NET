using System.Text;

namespace Slippi.NET.Utils;

internal class StringUtils
{
    public static StringUtils Instance = new StringUtils();

    private StringUtils()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public static Encoding Shift_JIS { get; } = Encoding.GetEncoding("Shift-JIS");

    public string? ReadShiftJIS(Span<byte> buffer)
    {
        return Shift_JIS.GetString(buffer.Slice(0, buffer.IndexOf((byte)0)));
    }

    public string? ReadUtf8(Span<byte> buffer)
    {
        return Encoding.UTF8.GetString(buffer.Slice(0, buffer.IndexOf((byte)0)));
    }
}
