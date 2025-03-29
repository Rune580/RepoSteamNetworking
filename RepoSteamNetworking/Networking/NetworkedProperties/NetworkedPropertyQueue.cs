using System.Collections.Generic;
using RepoSteamNetworking.Networking.Packets;

namespace RepoSteamNetworking.Networking.NetworkedProperties;

internal class NetworkedPropertyQueue
{
    private readonly Dictionary<uint, Dictionary<uint, Dictionary<ulong, object>>> _networkedPropertyValues = new();

    public object this[uint networkId, uint subId, ulong packedPropId]
    {
        set
        {
            if (!_networkedPropertyValues.TryGetValue(networkId, out var subPropertyValues))
                _networkedPropertyValues[subId] = subPropertyValues = new Dictionary<uint, Dictionary<ulong, object>>();

            if (!subPropertyValues.TryGetValue(subId, out var propertyValues))
                subPropertyValues[subId] = propertyValues = new Dictionary<ulong, object>();
            
            propertyValues[packedPropId] = value;
        }
    }

    public void SendData()
    {
        var packet = new NetworkedPropertiesDataPacket
        {
            NetworkedPropertyValues = _networkedPropertyValues,
        };
        
        // TODO: Send packet
        
        _networkedPropertyValues.Clear();
    }
}