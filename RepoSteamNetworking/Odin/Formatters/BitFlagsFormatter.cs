using RepoSteamNetworking.Utils;
using Sirenix.Serialization;

namespace RepoSteamNetworking.Odin.Formatters;

public class BitFlagsFormatter : MinimalBaseFormatter<BitFlags>
{
    private static readonly Serializer<byte> ByteSerializer = Serializer.Get<byte>();
    
    public override void Read(ref BitFlags value, IDataReader reader)
    {
        var length = ByteSerializer.ReadValue(reader);
        
        var bytes = new byte[length];
        for (var i = 0; i < length; i++)
            bytes[i] = ByteSerializer.ReadValue(reader);
        
        value.SetFromBytes(bytes);
    }

    public override void Write(ref BitFlags value, IDataWriter writer)
    {
        var bytes = value.AsByteArray();
        
        ByteSerializer.WriteValue((byte)bytes.Length, writer);
        foreach (var b in bytes)
            ByteSerializer.WriteValue(b, writer);
    }
}