using RepoSteamNetworking.API;
using RepoSteamNetworking.Networking.NetworkedProperties;
using RepoSteamNetworking.Networking.Serialization;

namespace RepoSteamNetworking.Networking.Packets;

internal class NetworkedPropertiesDataPacket : NetworkPacket<NetworkedPropertiesDataPacket>
{
    public uint NetworkId { get; set; }
    public BehaviourPropertyChanges[] BehaviourProperties { get; set; } = [];
    
    protected override void WriteData(SocketMessage socketMessage)
    {
        socketMessage.Write(NetworkId);
        
        socketMessage.Write(BehaviourProperties.Length);
        foreach (var property in BehaviourProperties)
            property.WriteData(socketMessage);
    }

    protected override void ReadData(SocketMessage socketMessage)
    {
        NetworkId = socketMessage.Read<uint>();

        var length = socketMessage.Read<int>();
        BehaviourProperties = new BehaviourPropertyChanges[length];
        
        for (int i = 0; i < length; i++)
        {
            var property = new BehaviourPropertyChanges();
            property.ReadData(socketMessage);
            
            BehaviourProperties[i] = property;
        }
    }
}