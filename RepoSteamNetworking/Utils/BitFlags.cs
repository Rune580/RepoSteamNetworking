using System;

namespace RepoSteamNetworking.Utils;

public struct BitFlags
{
    private bool[] _bits;

    public BitFlags(byte bits)
    {
        _bits = new bool[8];
        for (int i = 0; i < _bits.Length; i++)
            _bits[i] = (bits & (1 << i)) != 0;
    }

    public BitFlags(ushort bits)
    {
        _bits = new bool[16];
        for (int i = 0; i < _bits.Length; i++)
            _bits[i] = (bits & (1 << i)) != 0;
    }

    public BitFlags(byte[] bytes)
    {
        _bits = new bool[bytes.Length * 8];
        for (int i = 0; i < _bits.Length; i++)
        {
            var bytePos = i / 8;
            _bits[i] = (bytes[bytePos] & (1 << (i - 8 * bytePos))) != 0;
        }
    }

    public bool this[int index]
    {
        get => _bits[index];
        set
        {
            while (index >= _bits.Length)
                Array.Resize(ref _bits, _bits.Length * 2);
            
            _bits[index] = value;
        }
    }

    public void SetFromBytes(byte[] bytes)
    {
        _bits = new bool[bytes.Length * 8];
        for (int i = 0; i < _bits.Length; i++)
        {
            var bytePos = i / 8;
            _bits[i] = (bytes[bytePos] & (1 << (i - 8 * bytePos))) != 0;
        }
    }

    public byte[] AsByteArray()
    {
        var bytes = new byte[_bits.Length / 8];

        for (int i = 0; i < _bits.Length; i++)
        {
            var bytePos = i / 8;
            bytes[bytePos] |= (byte)((_bits[i] ? 1 : 0) << i);
        }
        
        return bytes;
    }
}