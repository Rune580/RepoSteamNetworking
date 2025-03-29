using System.Collections.Generic;
using RepoSteamNetworking.API;
using RepoSteamNetworking.Networking.Data;

namespace RepoSteamNetworking.Networking.Packets;

internal class NetworkedPropertiesDataPacket : NetworkPacket<NetworkedPropertiesDataPacket>
{
    public Dictionary<uint, Dictionary<uint, Dictionary<ulong, object>>> NetworkedPropertyValues { get; set; }
    
    protected override void WriteData(SocketMessage socketMessage)
    {
        foreach (var (networkId, subPropertyValues) in NetworkedPropertyValues)
        {
            foreach (var (subId, propertyValues) in subPropertyValues)
            {
                foreach (var (packedPropId, value) in propertyValues)
                {
                    
                }
            }
        }
    }

    protected override void ReadData(SocketMessage socketMessage)
    {
        
    }
}