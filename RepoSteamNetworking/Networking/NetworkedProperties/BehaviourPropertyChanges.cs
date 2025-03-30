using RepoSteamNetworking.Networking.Serialization;

namespace RepoSteamNetworking.Networking.NetworkedProperties;

public class BehaviourPropertyChanges
{
    public uint GuidPaletteId { get; set; }
    public uint SubId { get; set; }
    public uint BehaviourClassId { get; set; }
    public NetworkedPropertyChange[] PropertyChanges { get; set; } = [];
    
    public void WriteData(SocketMessage writer)
    {
        writer.Write(GuidPaletteId);
        writer.Write(SubId);
        writer.Write(BehaviourClassId);
        writer.Write(PropertyChanges.Length);
        
        foreach (var propertyChange in PropertyChanges)
        {
            writer.Write((byte)propertyChange.ChangeKind);
            writer.Write(propertyChange.PropertyId);
            writer.Write(propertyChange.Value);
        }
    }

    public void ReadData(SocketMessage reader)
    {
        GuidPaletteId = reader.Read<uint>();
        SubId = reader.Read<uint>();
        BehaviourClassId = reader.Read<uint>();
        
        var length = reader.Read<int>();
        var changes = new NetworkedPropertyChange[length];

        for (int i = 0; i < length; i++)
        {
            changes[i] = new NetworkedPropertyChange
            {
                ChangeKind = (VariableChangeKind)reader.Read<byte>(),
                PropertyId = reader.Read<uint>(),
                Value = reader.Read<object>()
            };
        }
        
        PropertyChanges = changes;
    }
}