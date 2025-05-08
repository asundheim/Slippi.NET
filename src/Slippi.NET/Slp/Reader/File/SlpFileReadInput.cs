using Slippi.NET.Slp.Reader.Types;

namespace Slippi.NET.Slp.Reader.File;

public class SlpFileReadInput : SlpReadInput
{
    public override string Source => SlpInputSource.FILE;
    public required string FilePath { get; set; }

    public override SlpRef GetRef()
    {
        return new SlpFileSourceRef(FilePath);
    }
}
