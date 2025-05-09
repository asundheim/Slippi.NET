using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace UBJson;

public class UBJsonReader
{
    private readonly BigEndianReader _reader;
    private readonly bool _draft12;

    public UBJsonReader(Stream stream, bool draft12 = true)
    {
        _reader = new BigEndianReader(stream);
        _draft12 = draft12;
    }

    public JToken Parse()
    {
        return Parse(_reader.ReadUInt8());
    }

    public static TObj Parse<TObj>(Stream stream)
    {
        UBJsonReader reader = new UBJsonReader(stream);

        return reader.Parse().ToObject<TObj>() ?? throw new InvalidDataException($"Failed to parse {typeof(TObj)} from stream");
    }

    private JToken Parse(byte type)
    {
        return (char)type switch
        {
            '[' => ParseArray(),
            '{' => ParseObject(),
            'Z' => new JValue(JValue.CreateNull()),
            'T' => new JValue(true),
            'B' => new JValue(_reader.ReadUInt8()),
            'U' => new JValue(_reader.ReadUInt8()),
            'i' => new JValue(_draft12 ? _reader.ReadInt16() : _reader.ReadUInt8()),
            'I' => new JValue(_draft12 ? _reader.ReadInt32() : _reader.ReadInt16()),
            'l' => new JValue(_reader.ReadInt32()),
            'L' => new JValue(_reader.ReadInt64()),
            'd' => new JValue(_reader.ReadSingle()),
            'D' => new JValue(_reader.ReadDouble()),
            's' or 'S' => new JValue(ParseString(type)),
            'a' or 'A' => ParseData(type),
            'C' => new JValue(_reader.ReadChar()),
            _ => throw new InvalidOperationException($"Unknown control code {(char)type}")
        };
    }

    private JArray ParseArray()
    {
        var result = new JArray();
        byte code = _reader.ReadUInt8();
        byte valueType = 0;

        if (code == '$')
        {
            valueType = _reader.ReadUInt8();
            code = _reader.ReadUInt8();
        }

        long size = -1;
        if (code == '#')
        {
            size = ParseSize(-1);
            if (size < 0)
            {
                throw new Exception($"Unknown type for code {(char)code}");
            }

            if (size == 0)
            {
                return result;
            }

            code = valueType == 0 ? _reader.ReadUInt8() : valueType;
        }

        long count = 0;
        while (_reader.Position != _reader.Length && code != ']')
        {
            JToken val = Parse(code);
            result.Add(val);

            count++;
            if (size > 0 && count >= size)
            {
                break;
            }

            code = valueType == 0 ? _reader.ReadUInt8() : valueType;
        }

        return result;
    }

    private JObject ParseObject()
    {
        var result = new JObject();
        byte code = _reader.ReadUInt8();
        byte valueType = 0;
        if (code == '$')
        {
            valueType = _reader.ReadUInt8();
            code = _reader.ReadUInt8();
        }

        long size = -1;
        if (code == '#')
        {
            size = ParseSize(-1);
            if (size < 0)
            {
                throw new Exception($"Unknown type for code {(char)code}");
            }

            if (size == 0)
            {
                return result;
            }

            code = _reader.ReadUInt8();
        }

        int count = 0;
        while (_reader.Position != _reader.Length && code != '}')
        {
            string key = ParseString(true, code);
            JToken child = Parse(valueType == 0 ? _reader.ReadUInt8() : valueType);

            result[key] = child;
            count++;
            if (size > 0 && count >= size)
            {
                break;
            }

            code = _reader.ReadUInt8();
        }

        return result;
    }

    private JArray ParseData(byte containerType)
    {
        byte dataType = _reader.ReadUInt8();
        uint size = containerType == 'A' ? _reader.ReadUInt32() : _reader.ReadUInt8();
        var result = new JArray();
        for (uint i = 0; i < size; i++)
        {
            JToken val = Parse(dataType);
            result.Add(val);
        }

        return result;
    }

    private string ParseString(byte type) => ParseString(isOptional: false, type);
    private string ParseString(bool isOptional, byte code)
    {
        long size = (char)code switch
        {
            'S' => ParseSize(-1),
            's' => _reader.ReadUInt8(),
            _ => isOptional ? ParseSize(code, -1) : throw new ArgumentException($"Unknown code {(char)code}, expecting string")
        };

        return size != 0 ? _reader.ReadStringUTF8(size) : string.Empty;
    }

    private long ParseSize(int defaultValue) => ParseSize(_reader.ReadUInt8(), defaultValue);
    private long ParseSize(byte type, long defaultValue)
    {
        return (char)type switch
        {
            'i' => _reader.ReadInt8(),
            'I' => _reader.ReadUInt16(),
            'l' => _reader.ReadUInt32(),
            'L' => _reader.ReadInt64(),
            'U' => _reader.ReadUInt8(),
            _ => defaultValue,
        };
    }
}