namespace Slippi.NET.Slp.Reader.Types;

public abstract class SlpReadInput
{
    public abstract string Source { get; }

    public abstract SlpRef GetRef();
}
