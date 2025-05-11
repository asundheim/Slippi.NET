using Slippi.NET.Slp.Reader.Types;

namespace Slippi.NET.Slp.Reader.Buffer;

public class SlpBufferReader : SlpReader
{
    public override string Source => SlpInputSource.BUFFER;
    public required byte[] Buffer { get; set; }

    protected override SlpRef GetRef()
    {
        return new SlpBufferRef() { Buffer = Buffer };
    }
}
