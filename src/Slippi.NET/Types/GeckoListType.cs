namespace Slippi.NET.Types;

public record class GeckoListType
{
    public GeckoListType(List<GeckoCodeType> codes, byte[] contents)
    {
        Codes = codes;
        Contents = contents;
    }

    public List<GeckoCodeType> Codes { get; set; } = new();
    public byte[] Contents { get; set; } = Array.Empty<byte>();
}