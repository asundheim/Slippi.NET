using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UBJson;

public class UBJsonWriter : IDisposable
{
    private readonly BigEndianWriter _writer;
    private readonly Stack<JsonObject> _stack = [];

    private JsonObject? _currentObj;
    private bool _currentObjHasName;

    public UBJsonWriter(Stream outputStream)
    {
        _writer = new BigEndianWriter(outputStream);
    }

    public static byte[] Encode(object obj)
    {
        JToken json = JToken.FromObject(obj);
        using MemoryStream stream = new MemoryStream(128);
        using UBJsonWriter writer = new UBJsonWriter(stream);
        writer.Value(json);
        writer.Flush();

        return stream.ToArray();
    }

    public void Dispose()
    {
        _writer?.Dispose();
    }

    public UBJsonWriter Object()
    {
        if (_currentObj != null && !_currentObj.IsArray)
        {
            if (!_currentObjHasName)
            {
                throw new Exception("Name must be set.");
            }

            _currentObjHasName = false;
        }

        _currentObj = new JsonObject(_writer, isArray: false);
        _stack.Push(_currentObj);

        return this;
    }

    public UBJsonWriter Object(string name)
    {
        Name(name).Object();

        return this;
    }

    public UBJsonWriter Array()
    {
        if (_currentObj != null)
        {
            if (!_currentObj.IsArray)
            {
                if (!_currentObjHasName)
                {
                    throw new Exception("Name must be set.");
                }

                _currentObjHasName = false;
            }
        }

        _stack.Push(_currentObj = new JsonObject(_writer, isArray: true));

        return this;
    }

    public UBJsonWriter Array(string name)
    {
        Name(name).Array();

        return this;
    }

    public UBJsonWriter Name(string name)
    {
        if (_currentObj is null || _currentObj.IsArray)
        {
            throw new Exception("Current item must be an object.");
        }

        byte[] bytes = Encoding.UTF8.GetBytes(name);
        if (bytes.Length <= byte.MaxValue)
        {
            _writer.Write((byte)'i');
            _writer.Write((byte)bytes.Length);
        }
        else if (bytes.Length <= short.MaxValue)
        {
            _writer.Write((byte)'I');
            _writer.WriteShort((short)bytes.Length);
        }
        else
        {
            _writer.Write((byte)'l');
            _writer.WriteInt(bytes.Length);
        }

        _writer.Write(bytes);
        _currentObjHasName = true;

        return this;
    }

    public UBJsonWriter Value(byte value)
    {
        CheckName();

        _writer.Write((byte)'i');
        _writer.Write(value);

        return this;
    }

    public UBJsonWriter Value(short value)
    {
        CheckName();

        _writer.Write((byte)'I');
        _writer.WriteShort(value);

        return this;
    }

    public UBJsonWriter Value(int value)
    {
        CheckName();

        _writer.Write((byte)'l');
        _writer.WriteInt(value);

        return this;
    }

    public UBJsonWriter Value(long value)
    {
        CheckName();

        _writer.Write((byte)'L');
        _writer.WriteLong(value);

        return this;
    }

    public UBJsonWriter Value(float value)
    {
        CheckName();

        _writer.Write((byte)'d');
        _writer.WriteFloat(value);

        return this;
    }

    public UBJsonWriter Value(double value)
    {
        CheckName();

        _writer.Write((byte)'D');
        _writer.WriteDouble(value);

        return this;
    }

    public UBJsonWriter Value(bool value)
    {
        CheckName();

        _writer.Write((byte)(value ? 'T' : 'F'));

        return this;
    }

    public UBJsonWriter Value(string value)
    {
        CheckName();

        _writer.Write((byte)'S');
        byte[] bytes = Encoding.UTF8.GetBytes(value);
        if (bytes.Length <= byte.MaxValue)
        {
            _writer.Write((byte)'i');
            _writer.Write((byte)bytes.Length);
        }
        else if (bytes.Length <= short.MaxValue)
        {
            _writer.Write((byte)'I');
            _writer.WriteShort((short)bytes.Length);
        }
        else
        {
            _writer.Write((byte)'l');
            _writer.WriteInt(bytes.Length);
        }

        _writer.Write(bytes);

        return this;
    }

    public UBJsonWriter Value(byte[] values)
    {
        Array();
        _writer.Write((byte)'$');
        _writer.Write((byte)'i');
        _writer.Write((byte)'#');

        Value(values.Length);
        for (int i = 0, n = values.Length; i < n; i++)
        {
            _writer.Write(values[i]);
        }

        Pop(true);

        return this;
    }

    public UBJsonWriter Value(short[] values)
    {
        Array();
        _writer.Write((byte)'$');
        _writer.Write((byte)'I');
        _writer.Write((byte)'#');

        Value(values.Length);
        for (int i = 0, n = values.Length; i < n; i++)
        {
            _writer.WriteShort(values[i]);
        }

        Pop(true);

        return this;
    }

    public UBJsonWriter Value(int[] values)
    {
        Array();
        _writer.Write((byte)'$');
        _writer.Write((byte)'l');
        _writer.Write((byte)'#');

        Value(values.Length);
        for (int i = 0, n = values.Length; i < n; i++)
        {
            _writer.WriteInt(values[i]);
        }

        Pop(true);

        return this;
    }

    public UBJsonWriter Value(long[] values)
    {
        Array();
        _writer.Write((byte)'$');
        _writer.Write((byte)'L');
        _writer.Write((byte)'#');

        Value(values.Length);
        for (int i = 0, n = values.Length; i < n; i++)
        {
            _writer.WriteLong(values[i]);
        }

        Pop(true);

        return this;
    }

    public UBJsonWriter Value(float[] values)
    {
        Array();
        _writer.Write((byte)'$');
        _writer.Write((byte)'d');
        _writer.Write((byte)'#');

        Value(values.Length);
        for (int i = 0, n = values.Length; i < n; i++)
        {
            _writer.WriteFloat(values[i]);
        }

        Pop(true);

        return this;
    }

    public UBJsonWriter Value(double[] values)
    {
        Array();
        _writer.Write((byte)'$');
        _writer.Write((byte)'D');
        _writer.Write((byte)'#');

        Value(values.Length);
        for (int i = 0, n = values.Length; i < n; i++)
        {
            _writer.WriteDouble(values[i]);
        }

        Pop(true);

        return this;
    }

    public UBJsonWriter Value(bool[] values)
    {
        Array();
        for (int i = 0, n = values.Length; i < n; i++)
        {
            _writer.Write((byte)(values[i] ? 'T' : 'F'));
        }

        Pop();
        return this;
    }

    public UBJsonWriter Value(char[] values)
    {
        Array();
        _writer.Write((byte)'$');
        _writer.Write((byte)'C');
        _writer.Write((byte)'#');

        Value(values.Length);
        for (int i = 0, n = values.Length; i < n; i++)
        {
            _writer.WriteShort((short)values[i]);
        }

        Pop(true);

        return this;
    }

    public UBJsonWriter Value(string[] values)
    {
        Array();
        _writer.Write((byte)'$');
        _writer.Write((byte)'S');
        _writer.Write((byte)'#');

        Value(values.Length);
        for (int i = 0, n = values.Length; i < n; i++)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(values[i]);
            if (bytes.Length <= byte.MaxValue)
            {
                _writer.Write((byte)'i');
                _writer.Write((byte)bytes.Length);
            }
            else if (bytes.Length <= short.MaxValue)
            {
                _writer.Write((byte)'I');
                _writer.WriteShort((short)bytes.Length);
            }
            else
            {
                _writer.Write((byte)'l');
                _writer.WriteInt(bytes.Length);
            }
            _writer.Write(bytes);
        }

        Pop(true);

        return this;
    }

    public UBJsonWriter Value(JToken value, string? name = null)
    {
        switch (value.Type)
        {
            case JTokenType.Object:
                if (name != null)
                {
                    Object(name);
                }
                else
                {
                    Object();
                }

                var obj = (JObject)value;
                foreach (var pair in obj)
                {
                    Value(pair.Value!, pair.Key);
                }

                Pop();
                break;
            case JTokenType.Property:
                if (name != null)
                {
                    Object(name);
                }
                else
                {
                    Object();
                }

                var prop = (JProperty)value;
                Value(prop.Value, prop.Name);

                Pop();
                break;
            case JTokenType.Array:
                if (name != null)
                {
                    Array(name);
                }
                else
                {
                    Array();
                }

                foreach (var child in value.Children())
                {
                    Value(child);
                }

                Pop();
                break;
            case JTokenType.Integer:
                if (name != null)
                {
                    Name(name);
                }

                Value((long)value);
                break;
            case JTokenType.Float:
                if (name != null)
                {
                    Name(name);
                }

                Value((float)value);
                break;
            case JTokenType.String:
                if (name != null)
                {
                    Name(name);
                }

                Value((string)value!);
                break;
            case JTokenType.Boolean:
                if (name != null)
                {
                    Name(name);
                }

                Value((bool)value);
                break;
            case JTokenType.Null:
                if (name != null)
                {
                    Name(name);
                }

                Value();
                break;
            default:
                throw new Exception($"Unhandled JValue type {value.Type}");
        }

        return this;
    }

    public UBJsonWriter Value()
    {
        CheckName();

        _writer.Write((byte)'Z');

        return this;
    }

    public UBJsonWriter Set(string name, byte value)
    {
        return Name(name).Value(value);
    }

    public UBJsonWriter Set(string name, short value)
    {
        return Name(name).Value(value);
    }

    public UBJsonWriter Set(string name, int value)
    {
        return Name(name).Value(value);
    }

    public UBJsonWriter Set(string name, long value)
    {
        return Name(name).Value(value);
    }

    public UBJsonWriter Set(string name, float value)
    {
        return Name(name).Value(value);
    }

    public UBJsonWriter Set(string name, double value)
    {
        return Name(name).Value(value);
    }

    public UBJsonWriter Set(string name, bool value)
    {
        return Name(name).Value(value);
    }

    public UBJsonWriter Set(string name, char value)
    {
        return Name(name).Value((short)value);
    }

    public UBJsonWriter Set(string name, string value)
    {
        return Name(name).Value(value);
    }

    public UBJsonWriter Set(string name, byte[] value)
    {
        return Name(name).Value(value);
    }

    public UBJsonWriter Set(string name, short[] value)
    {
        return Name(name).Value(value);
    }

    public UBJsonWriter Set(string name, int[] value)
    {
        return Name(name).Value(value);
    }

    public UBJsonWriter Set(string name, long[] value)
    {
        return Name(name).Value(value);
    }

    public UBJsonWriter Set(string name, float[] value)
    {
        return Name(name).Value(value);
    }

    public UBJsonWriter Set(string name, double[] value)
    {
        return Name(name).Value(value);
    }

    public UBJsonWriter Set(string name, bool[] value)
    {
        return Name(name).Value(value);
    }

    public UBJsonWriter Set(string name, char[] value)
    {
        return Name(name).Value(value);
    }

    public UBJsonWriter Set(string name, String[] value)
    {
        return Name(name).Value(value);
    }

    public UBJsonWriter Set(string name)
    {
        return Name(name).Value();
    }

    private void CheckName()
    {
        if (_currentObj is not null)
        {
            if (!_currentObj.IsArray)
            {
                if (!_currentObjHasName)
                {
                    throw new Exception("Name must be set.");
                }

                _currentObjHasName = false;
            }
        }
    }

    public UBJsonWriter Pop()
    {
        return Pop(false);
    }

    protected UBJsonWriter Pop(bool keepObjectOpen)
    {
        if (_currentObjHasName)
        {
            throw new Exception("Expected an object, array, or value since a name was set.");
        }

        if (keepObjectOpen)
        {
            _stack.Pop();
        }
        else
        {
            _stack.Pop().WriteObjectClose();
        }

        _currentObj = _stack.Count == 0 ? null : _stack.Peek();
        return this;
    }

    public void Flush()
    {
        _writer.Flush();
    }
}
