namespace Slippi.NET.Types;

public record class GeckoCodeType
{
    public GeckoCodeType(int? type, int? address, byte[] contents)
    {
        Type = type;
        Address = address;
        Contents = contents;
    }

    public int? Type { get; set; }
    public int? Address { get; set; }
    public byte[] Contents { get; set; } = Array.Empty<byte>();
}