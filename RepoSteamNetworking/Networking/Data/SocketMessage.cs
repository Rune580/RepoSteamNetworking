using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RepoSteamNetworking.Networking.Data;

public class SocketMessage
{
    private readonly List<byte> _writeBuffer = [];
    private readonly byte[] _readBuffer = [];

    private int _pos;

    public SocketMessage()
    {
        
    }

    public SocketMessage(byte[] buffer)
    {
        _readBuffer = buffer;
    }
    
    public void ResetPosition() => _pos = 0;

    internal byte[] GetBytes() => _readBuffer.Length > 0 ? _readBuffer : _writeBuffer.ToArray();

    #region Write Methods

    public SocketMessage WriteByte(byte value)
    {
        _writeBuffer.Add(value);
        return this;
    }
    
    public SocketMessage WriteByteArray(byte[] value)
    {
        WriteInt(value.Length);
        _writeBuffer.AddRange(value);
        return this;
    }

    public SocketMessage WriteSByte(sbyte value)
    {
        unchecked
        {
            _writeBuffer.Add((byte)value);
        }
        return this;
    }

    public SocketMessage WriteSByteArray(sbyte[] value)
    {
        WriteInt(value.Length);
        unchecked
        {
            _writeBuffer.AddRange(value.Select(signedByte => (byte)signedByte));
        }
        return this;
    }
    
    public SocketMessage WriteUShort(ushort value)
    {
        _writeBuffer.AddRange(BitConverter.GetBytes(value));
        return this;
    }
    
    public SocketMessage WriteUShortArray(ushort[] value)
    {
        WriteInt(value.Length);
        _writeBuffer.AddRange(value.SelectMany(BitConverter.GetBytes));
        return this;
    }

    public SocketMessage WriteShort(short value)
    {
        _writeBuffer.AddRange(BitConverter.GetBytes(value));
        return this;
    }
    
    public SocketMessage WriteShortArray(short[] value)
    {
        WriteInt(value.Length);
        _writeBuffer.AddRange(value.SelectMany(BitConverter.GetBytes));
        return this;
    }

    public SocketMessage WriteUInt(uint value)
    {
        _writeBuffer.AddRange(BitConverter.GetBytes(value));
        return this;
    }
    
    public SocketMessage WriteUIntArray(uint[] value)
    {
        WriteInt(value.Length);
        _writeBuffer.AddRange(value.SelectMany(BitConverter.GetBytes));
        return this;
    }

    public SocketMessage WriteInt(int value)
    {
        _writeBuffer.AddRange(BitConverter.GetBytes(value));
        return this;
    }
    
    public SocketMessage WriteIntArray(int[] value)
    {
        WriteInt(value.Length);
        _writeBuffer.AddRange(value.SelectMany(BitConverter.GetBytes));
        return this;
    }

    public SocketMessage WriteULong(ulong value)
    {
        _writeBuffer.AddRange(BitConverter.GetBytes(value));
        return this;
    }

    public SocketMessage WriteULongArray(ulong[] value)
    {
        WriteInt(value.Length);
        _writeBuffer.AddRange(value.SelectMany(BitConverter.GetBytes));
        return this;
    }

    public SocketMessage WriteLong(long value)
    {
        _writeBuffer.AddRange(BitConverter.GetBytes(value));
        return this;
    }

    public SocketMessage WriteLongArray(long[] value)
    {
        WriteInt(value.Length);
        _writeBuffer.AddRange(value.SelectMany(BitConverter.GetBytes));
        return this;
    }

    public SocketMessage WriteFloat(float value)
    {
        _writeBuffer.AddRange(BitConverter.GetBytes(value));
        return this;
    }

    public SocketMessage WriteFloatArray(float[] value)
    {
        WriteInt(value.Length);
        _writeBuffer.AddRange(value.SelectMany(BitConverter.GetBytes));
        return this;
    }

    public SocketMessage WriteDouble(double value)
    {
        _writeBuffer.AddRange(BitConverter.GetBytes(value));
        return this;
    }

    public SocketMessage WriteDoubleArray(double[] value)
    {
        WriteInt(value.Length);
        _writeBuffer.AddRange(value.SelectMany(BitConverter.GetBytes));
        return this;
    }

    public SocketMessage WriteBool(bool value)
    {
        _writeBuffer.AddRange(BitConverter.GetBytes(value));
        return this;
    }

    public SocketMessage WriteBoolArray(bool[] value)
    {
        WriteInt(value.Length);
        _writeBuffer.AddRange(value.SelectMany(BitConverter.GetBytes));
        return this;
    }

    public SocketMessage WriteString(string value, Encoding? encoding = null)
    {
        encoding ??= Encoding.UTF8;
        var strBytes = encoding.GetBytes(value);
        
        WriteInt(strBytes.Length);
        _writeBuffer.AddRange(strBytes);
        return this;
    }

    public SocketMessage WriteStringArray(string[] value, Encoding? encoding = null)
    {
        WriteInt(value.Length);

        encoding ??= Encoding.UTF8;
        foreach (var text in value)
            WriteString(text, encoding);
        
        return this;
    }

    public SocketMessage WriteVector2(Vector2 value)
    {
        WriteFloat(value.x);
        WriteFloat(value.y);
        return this;
    }

    public SocketMessage WriteVector2Array(Vector2[] value)
    {
        WriteInt(value.Length);
        foreach (var vector in value)
            WriteVector2(vector);

        return this;
    }

    public SocketMessage WriteVector2Int(Vector2Int value)
    {
        WriteInt(value.x);
        WriteInt(value.y);
        return this;
    }

    public SocketMessage WriteVector2IntArray(Vector2Int[] value)
    {
        WriteInt(value.Length);
        foreach (var vector in value)
            WriteVector2Int(vector);
        
        return this;
    }

    public SocketMessage WriteVector3(Vector3 value)
    {
        WriteFloat(value.x);
        WriteFloat(value.y);
        WriteFloat(value.z);
        return this;
    }

    public SocketMessage WriteVector3Array(Vector3[] value)
    {
        WriteInt(value.Length);
        foreach (var vector in value)
            WriteVector3(vector);
        
        return this;
    }

    public SocketMessage WriteVector3Int(Vector3Int value)
    {
        WriteInt(value.x);
        WriteInt(value.y);
        WriteInt(value.z);
        return this;
    }

    public SocketMessage WriteVector3IntArray(Vector3Int[] value)
    {
        WriteInt(value.Length);
        foreach (var vector in value)
            WriteVector3Int(vector);
        
        return this;
    }

    public SocketMessage WriteVector4(Vector4 value)
    {
        WriteFloat(value.x);
        WriteFloat(value.y);
        WriteFloat(value.z);
        WriteFloat(value.w);
        return this;
    }

    public SocketMessage WriteVector4Array(Vector4[] value)
    {
        WriteInt(value.Length);
        foreach (var vector in value)
            WriteVector4(vector);
        
        return this;
    }

    public SocketMessage WriteQuaternion(Quaternion value)
    {
        WriteFloat(value.x);
        WriteFloat(value.y);
        WriteFloat(value.z);
        WriteFloat(value.w);
        return this;
    }

    public SocketMessage WriteQuaternionArray(Quaternion[] value)
    {
        WriteInt(value.Length);
        foreach (var quaternion in value)
            WriteQuaternion(quaternion);
        return this;
    }

    public SocketMessage WriteColor(Color value)
    {
        WriteFloat(value.r);
        WriteFloat(value.g);
        WriteFloat(value.b);
        WriteFloat(value.a);
        return this;
    }

    public SocketMessage WriteColorArray(Color[] value)
    {
        WriteInt(value.Length);
        foreach (var color in value)
            WriteColor(color);

        return this;
    }
    
    public SocketMessage WriteDictionary(IDictionary dict)
    {
        WriteInt(dict.Count);
        foreach (var key in dict.Keys)
        {
            WriteObject(key);
            WriteObject(dict[key]);
        }
        
        return this;
    }
    
    public SocketMessage WriteObject(object obj)
    {
        return obj switch
        {
            byte v => WriteByte(v),
            byte[] v => WriteByteArray(v),
            sbyte v => WriteSByte(v),
            sbyte[] v => WriteSByteArray(v),
            ushort v => WriteUShort(v),
            ushort[] v => WriteUShortArray(v),
            short v => WriteShort(v),
            short[] v => WriteShortArray(v),
            uint v => WriteUInt(v),
            uint[] v => WriteUIntArray(v),
            int v => WriteInt(v),
            int[] v => WriteIntArray(v),
            ulong v => WriteULong(v),
            ulong[] v => WriteULongArray(v),
            long v => WriteLong(v),
            long[] v => WriteLongArray(v),
            float v => WriteFloat(v),
            float[] v => WriteFloatArray(v),
            double v => WriteDouble(v),
            double[] v => WriteDoubleArray(v),
            bool v => WriteBool(v),
            bool[] v => WriteBoolArray(v),
            string v => WriteString(v),
            string[] v => WriteStringArray(v),
            Vector2 v => WriteVector2(v),
            Vector2[] v => WriteVector2Array(v),
            Vector2Int v => WriteVector2Int(v),
            Vector2Int[] v => WriteVector2IntArray(v),
            Vector3 v => WriteVector3(v),
            Vector3[] v => WriteVector3Array(v),
            Vector3Int v => WriteVector3Int(v),
            Vector3Int[] v => WriteVector3IntArray(v),
            Vector4 v => WriteVector4(v),
            Vector4[] v => WriteVector4Array(v),
            Quaternion v => WriteQuaternion(v),
            Quaternion[] v => WriteQuaternionArray(v),
            Color v => WriteColor(v),
            Color[] v => WriteColorArray(v),
            IDictionary d => WriteDictionary(d),
            _ => throw new Exception($"No compatible writer for {obj.GetType()}!")
        };
    }

    #endregion

    #region Read Methods

    public byte ReadByte()
    {
        return _readBuffer[_pos++];
    }
    
    public SocketMessage ReadByteArray()
    {
        var length = ReadInt();
        
        var data = new byte[length];
        Array.Copy(_readBuffer, _pos, data, 0, length);
        _pos += length;
        
        return this;
    }

    public sbyte ReadSByte()
    {
        unchecked
        {
            return (sbyte)_readBuffer[_pos++];
        }
    }

    public sbyte[] ReadSByteArray()
    {
        var length = ReadInt();
        unchecked
        {
            var data = new sbyte[length];
            
            for (int i = 0; i < length; i++)
            {
                var value = _readBuffer[_pos++];
                data[i] = (sbyte)value;
            }
            
            return data;
        }
    }
    
    public ushort ReadUShort()
    {
        var value = BitConverter.ToUInt16(_readBuffer, _pos);
        _pos += sizeof(ushort);
        return value;
    }
    
    public ushort[] ReadUShortArray()
    {
        var length = ReadInt();
        var data = new ushort[length];

        for (int i = 0; i < length; i++)
        {
            data[i] = BitConverter.ToUInt16(_readBuffer, _pos);
            _pos += sizeof(ushort);
        }
        
        return data;
    }

    public short ReadShort()
    {
        var value = BitConverter.ToInt16(_readBuffer, _pos);
        _pos += sizeof(short);
        return value;
    }
    
    public short[] ReadShortArray()
    {
        var length = ReadInt();
        var data = new short[length];

        for (int i = 0; i < length; i++)
        {
            data[i] = BitConverter.ToInt16(_readBuffer, _pos);
            _pos += sizeof(short);
        }
        
        return data;
    }

    public uint ReadUInt()
    {
        var value = BitConverter.ToUInt32(_readBuffer, _pos);
        _pos += sizeof(uint);
        return value;
    }
    
    public uint[] ReadUIntArray()
    {
        var length = ReadInt();
        var data = new uint[length];

        for (int i = 0; i < length; i++)
        {
            data[i] = BitConverter.ToUInt32(_readBuffer, _pos);
            _pos += sizeof(uint);
        }
        
        return data;
    }

    public int ReadInt()
    {
        var value = BitConverter.ToInt32(_readBuffer, _pos);
        _pos += sizeof(int);
        return value;
    }
    
    public int[] ReadIntArray()
    {
        var length = ReadInt();
        var data = new int[length];

        for (int i = 0; i < length; i++)
        {
            data[i] = BitConverter.ToInt32(_readBuffer, _pos);
            _pos += sizeof(int);
        }
        
        return data;
    }

    public ulong ReadULong()
    {
        var value = BitConverter.ToUInt64(_readBuffer, _pos);
        _pos += sizeof(ulong);
        return value;
    }

    public ulong[] ReadULongArray()
    {
        var length = ReadInt();
        var data = new ulong[length];

        for (int i = 0; i < length; i++)
        {
            data[i] = BitConverter.ToUInt64(_readBuffer, _pos);
            _pos += sizeof(ulong);
        }
        
        return data;
    }

    public long ReadLong()
    {
        var value = BitConverter.ToInt64(_readBuffer, _pos);
        _pos += sizeof(long);
        return value;
    }

    public long[] ReadLongArray()
    {
        var length = ReadInt();
        var data = new long[length];

        for (int i = 0; i < length; i++)
        {
            data[i] = BitConverter.ToInt64(_readBuffer, _pos);
            _pos += sizeof(long);
        }
        
        return data;
    }

    public float ReadFloat()
    {
        var value = BitConverter.ToSingle(_readBuffer, _pos);
        _pos += sizeof(float);
        return value;
    }

    public float[] ReadFloatArray()
    {
        var length = ReadInt();
        var data = new float[length];

        for (int i = 0; i < length; i++)
        {
            data[i] = BitConverter.ToSingle(_readBuffer, _pos);
            _pos += sizeof(float);
        }
        
        return data;
    }

    public double ReadDouble()
    {
        var value = BitConverter.ToDouble(_readBuffer, _pos);
        _pos += sizeof(float);
        return value;
    }

    public double[] ReadDoubleArray()
    {
        var length = ReadInt();
        var data = new double[length];

        for (int i = 0; i < length; i++)
        {
            data[i] = BitConverter.ToDouble(_readBuffer, _pos);
            _pos += sizeof(double);
        }
        
        return data;
    }

    public bool ReadBool()
    {
        var value = BitConverter.ToBoolean(_readBuffer, _pos);
        _pos += sizeof(bool);
        return value;
    }

    public bool[] ReadBoolArray()
    {
        var length = ReadInt();
        var data = new bool[length];

        for (int i = 0; i < length; i++)
        {
            data[i] = BitConverter.ToBoolean(_readBuffer, _pos);
            _pos += sizeof(bool);
        }
        
        return data;
    }

    public string ReadString(Encoding? encoding = null)
    {
        var length = ReadInt();
        
        encoding ??= Encoding.UTF8;
        var str = encoding.GetString(_readBuffer, _pos, length);
        _pos += length;
        
        return str;
    }

    public string[] ReadStringArray(Encoding? encoding = null)
    {
        var arrayLength = ReadInt();
        
        var strArray = new string[arrayLength];
        encoding ??= Encoding.UTF8;

        for (int i = 0; i < arrayLength; i++)
        {
            var length = ReadInt();
            strArray[i] = encoding.GetString(_readBuffer, _pos, length);
            _pos += length;
        }

        return strArray;
    }

    public Vector2 ReadVector2()
    {
        return new Vector2(ReadFloat(), ReadFloat());
    }

    public Vector2[] ReadVector2Array()
    {
        var length = ReadInt();
        
        var data = new Vector2[length];
        for (int i = 0; i < length; i++)
            data[i] = ReadVector2();

        return data;
    }

    public Vector2Int ReadVector2Int()
    {
        return new Vector2Int(ReadInt(), ReadInt());
    }

    public Vector2Int[] ReadVector2IntArray()
    {
        var length = ReadInt();
        
        var data = new Vector2Int[length];
        for (int i = 0; i < length; i++)
            data[i] = ReadVector2Int();

        return data;
    }

    public Vector3 ReadVector3()
    {
        return new Vector3(ReadFloat(), ReadFloat(), ReadFloat());
    }

    public Vector3[] ReadVector3Array()
    {
        var length = ReadInt();
        
        var data = new Vector3[length];
        for (int i = 0; i < length; i++)
            data[i] = ReadVector3();

        return data;
    }

    public Vector3Int ReadVector3Int()
    {
        return new Vector3Int(ReadInt(), ReadInt(), ReadInt());
    }

    public Vector3Int[] ReadVector3IntArray()
    {
        var length = ReadInt();
        
        var data = new Vector3Int[length];
        for (int i = 0; i < length; i++)
            data[i] = ReadVector3Int();

        return data;
    }

    public Vector4 ReadVector4()
    {
        return new Vector4(ReadFloat(), ReadFloat(), ReadFloat(), ReadFloat());
    }

    public Vector4[] ReadVector4Array()
    {
        var length = ReadInt();
        
        var data = new Vector4[length];
        for (int i = 0; i < length; i++)
            data[i] = ReadVector4();

        return data;
    }

    public Quaternion ReadQuaternion()
    {
        return new Quaternion(ReadFloat(), ReadFloat(), ReadFloat(), ReadFloat());
    }

    public Quaternion[] ReadQuaternionArray()
    {
        var length = ReadInt();
        
        var data = new Quaternion[length];
        for (int i = 0; i < length; i++)
            data[i] = ReadQuaternion();

        return data;
    }

    public Color ReadColor()
    {
        return new Color(ReadFloat(), ReadFloat(), ReadFloat(), ReadFloat());
    }

    public Color[] ReadColorArray()
    {
        var length = ReadInt();
        
        var data = new Color[length];
        for (int i = 0; i < length; i++)
            data[i] = ReadColor();

        return data;
    }
    
    public IDictionary ReadDictionary(Type keyType, Type valueType)
    {
        var itemCount = ReadInt();

        var dict = new Dictionary<object, object>();

        for (int i = 0; i < itemCount; i++)
        {
            var key = ReadObject(keyType);
            var value = ReadObject(valueType);
            
            dict[key] = value;
        }
        
        return dict;
    }

    // This is also not the greatest bit of code, I don't care.
    public Dictionary<TKey, TValue> ReadDictionary<TKey, TValue>()
    {
        var dict = ReadDictionary(typeof(TKey), typeof(TValue));
        
        var typedDict = new Dictionary<TKey, TValue>();

        foreach (DictionaryEntry dictionaryEntry in dict)
            typedDict[(TKey)dictionaryEntry.Key] = (TValue)dictionaryEntry.Value;
        
        return typedDict;
    }
    
    // Yes I know this code is fairly dogshit. I don't care, it works fine.
    public object ReadObject(Type objectType)
    {
        if (objectType == typeof(byte))
        {
            if (objectType.IsArray)
                return ReadByteArray();

            return ReadByte();
        }
        
        if (objectType == typeof(sbyte))
        {
            if (objectType.IsArray)
                return ReadSByteArray();

            return ReadSByte();
        }
        
        if (objectType == typeof(ushort))
        {
            if (objectType.IsArray)
                return ReadUShortArray();

            return ReadUShort();
        }
        
        if (objectType == typeof(short))
        {
            if (objectType.IsArray)
                return ReadShortArray();

            return ReadShort();
        }
        
        if (objectType == typeof(uint))
        {
            if (objectType.IsArray)
                return ReadUIntArray();

            return ReadUInt();
        }
        
        if (objectType == typeof(int))
        {
            if (objectType.IsArray)
                return ReadIntArray();

            return ReadInt();
        }
        
        if (objectType == typeof(ulong))
        {
            if (objectType.IsArray)
                return ReadULongArray();

            return ReadULong();
        }
        
        if (objectType == typeof(long))
        {
            if (objectType.IsArray)
                return ReadLongArray();

            return ReadLong();
        }
        
        if (objectType == typeof(float))
        {
            if (objectType.IsArray)
                return ReadFloatArray();

            return ReadFloat();
        }
        
        if (objectType == typeof(double))
        {
            if (objectType.IsArray)
                return ReadDoubleArray();

            return ReadDouble();
        }
        
        if (objectType == typeof(bool))
        {
            if (objectType.IsArray)
                return ReadBoolArray();

            return ReadBool();
        }
        
        if (objectType == typeof(string))
        {
            if (objectType.IsArray)
                return ReadStringArray();

            return ReadString();
        }
        
        if (objectType == typeof(Vector2))
        {
            if (objectType.IsArray)
                return ReadVector2Array();

            return ReadVector2();
        }
        
        if (objectType == typeof(Vector2Int))
        {
            if (objectType.IsArray)
                return ReadVector2IntArray();

            return ReadVector2Int();
        }
        
        if (objectType == typeof(Vector3))
        {
            if (objectType.IsArray)
                return ReadVector3Array();

            return ReadVector3();
        }
        
        if (objectType == typeof(Vector3Int))
        {
            if (objectType.IsArray)
                return ReadVector3IntArray();

            return ReadVector3Int();
        }
        
        if (objectType == typeof(Vector4))
        {
            if (objectType.IsArray)
                return ReadVector4Array();

            return ReadVector4();
        }
        
        if (objectType == typeof(Quaternion))
        {
            if (objectType.IsArray)
                return ReadQuaternionArray();

            return ReadQuaternion();
        }
        
        if (objectType == typeof(Color))
        {
            if (objectType.IsArray)
                return ReadColorArray();

            return ReadColor();
        }

        if (typeof(IDictionary).IsAssignableFrom(objectType))
        {
            var types = objectType.GetGenericArguments();
            
            return ReadDictionary(types[0], types[1]);
        }

        throw new Exception($"No compatible reader for {objectType}!");
    }

    #endregion
}