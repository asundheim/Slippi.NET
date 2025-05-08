using Slippi.NET.Slp.Reader.Types;

namespace Slippi.NET.Slp.Reader.Buffer;

public class SlpBufferReadInput : SlpReadInput
{
    public override string Source => SlpInputSource.BUFFER;
    public required byte[] Buffer { get; set; }

    public override SlpRef GetRef()
    {
        return new SlpBufferSourceRef() { Buffer = Buffer };
    }
}
