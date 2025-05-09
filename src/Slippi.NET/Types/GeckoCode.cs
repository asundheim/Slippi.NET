namespace Slippi.NET.Types;

public record class GeckoCode
{
    public GeckoCode(uint? type, uint? address, byte[] contents)
    {
        Type = type;
        Address = address;
        Contents = contents;
    }

    public uint? Type { get; set; }
    public uint? Address { get; set; }
    public byte[] Contents { get; set; }
}
