namespace Slippi.NET.Slp.Reader;

/// <summary>
/// Abstract reader view over an <see cref="SlpFile"/> that obtains
/// an <see cref="SlpRef"/> and creates a new <see cref="SlpFile"/>.
/// </summary>
public abstract class SlpReader
{
    /// <summary>
    /// The type of backing source for this reader.
    /// </summary>
    public abstract string Source { get; }

    protected abstract SlpRef GetRef();

    public SlpFile OpenSlpFile()
    {
        SlpRef slpRef = GetRef();

        int rawDataPosition = slpRef.GetRawDataPosition();
        int rawDataLength = slpRef.GetRawDataLength(rawDataPosition);
        int metadataPosition = rawDataPosition + rawDataLength + 10; // remove metadata string
        int metadataLength = slpRef.GetMetadataLength(metadataPosition);
        var messaqeSizes = slpRef.GetMessageSizes(rawDataPosition);

        // transferring disposal ownership of SlpRef to the SlpFile
        return new SlpFile()
        {
            SlpRef = slpRef,
            RawDataPosition = rawDataPosition,
            RawDataLength = rawDataLength,
            MetadataPosition = metadataPosition,
            MetadataLength = metadataLength,
            MessageSizes = messaqeSizes
        };
    }
}
