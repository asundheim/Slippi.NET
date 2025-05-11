using Slippi.NET.Slp.Reader.Types;
using System.Diagnostics.CodeAnalysis;

namespace Slippi.NET.Slp.Reader.File;

public class SlpFileReader : SlpReader
{
    public SlpFileReader() { }

    [SetsRequiredMembers]
    public SlpFileReader(string filePath)
    {
        FilePath = filePath;
    }

    public override string Source => SlpInputSource.FILE;
    public required string FilePath { get; set; }

    protected override SlpRef GetRef()
    {
        return new SlpFileRef(FilePath);
    }
}
