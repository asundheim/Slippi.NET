namespace Slippi.NET.Types;

public record class GeckoCodeList
{
    public GeckoCodeList(List<GeckoCode> codes, byte[] contents)
    {
        Codes = codes;
        Contents = contents;
    }

    public List<GeckoCode> Codes { get; set; } = new();
    public byte[] Contents { get; set; } = Array.Empty<byte>();
}