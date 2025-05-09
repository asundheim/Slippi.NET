using System.IO;

namespace UBJson;

public class JsonObject
{
    private readonly BinaryWriter _outputWriter;
    public bool IsArray { get; }

    public JsonObject(BinaryWriter writer, bool isArray)
    {
        IsArray = isArray;

        _outputWriter = writer;
        _outputWriter.Write((byte)(isArray ? '[' : '{'));
    }

    public void WriteObjectClose()
    {
        _outputWriter.Write((byte)(IsArray ? ']' : '}'));
    }
}