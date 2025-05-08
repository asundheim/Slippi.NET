using Slippi.NET.Slp.Reader.Types;

namespace Slippi.NET.Slp.Reader;

public static class SlpReader
{
    public static SlpFile OpenSlpFile(SlpReadInput input)
    {
        SlpRef slpRef = input.GetRef();

        int rawDataPosition = slpRef.GetRawDataPosition();
        int rawDataLength = slpRef.GetRawDataLength(rawDataPosition);
        int metadataPosition = rawDataPosition + rawDataLength + 10; // remove metadata string
        int metadataLength = slpRef.GetMetadataLength(metadataPosition);
        var messaqeSizes = slpRef.GetMessageSizes(rawDataPosition);

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
