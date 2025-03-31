using System.Collections.Generic;
using System.Threading;
using RepoSteamNetworking.API;
using RepoSteamNetworking.Networking.Data;
using RepoSteamNetworking.Networking.Packets;
using RepoSteamNetworking.Utils;

namespace RepoSteamNetworking.Networking.NetworkedProperties;

internal class NetworkedPropertyQueue
{
    private readonly Dictionary<uint, Dictionary<PaletteSubId, Dictionary<BehaviourPropertyId, NetworkedPropertyChange>>> _networkedPropertyValues = new();
    
    public bool IsEmpty { get; private set; }

    public NetworkedPropertyChange this[uint networkId, PaletteSubId paletteSubId, BehaviourPropertyId behaviourPropId]
    {
        set
        {
            IsEmpty = false;
            
            if (!_networkedPropertyValues.TryGetValue(networkId, out var subPropertyValues))
                _networkedPropertyValues[networkId] = subPropertyValues = new Dictionary<PaletteSubId, Dictionary<BehaviourPropertyId, NetworkedPropertyChange>>();

            if (!subPropertyValues.TryGetValue(paletteSubId, out var propertyValues))
            {
                subPropertyValues[paletteSubId] = propertyValues = new Dictionary<BehaviourPropertyId, NetworkedPropertyChange>();
                
                if (value.ChangeKind is VariableChangeKind.Delta)
                {
                    propertyValues[behaviourPropId] = value;
                    return;
                }
            }

            if (value.ChangeKind is VariableChangeKind.Delta)
            {
                var currentProp = propertyValues[behaviourPropId];
                var newValue = VariableDeltaHelper.Add(currentProp.Value, value.Value);
                currentProp.Value = newValue;
                propertyValues[behaviourPropId] = currentProp;
                return;
            }
            
            propertyValues[behaviourPropId] = value;
        }
    }

    public void SendData()
    {
        if (!IsEmpty)
            return;

        var networkPropValues = new Dictionary<uint, Dictionary<PaletteSubId, Dictionary<BehaviourPropertyId, NetworkedPropertyChange>>>();
        
        foreach (var (networkId, subPropertyValues) in _networkedPropertyValues)
            networkPropValues[networkId] = subPropertyValues;
        
        _networkedPropertyValues.Clear();
        IsEmpty = true;

        // Handle sending data in separate thread to reduce overhead in calling threads.
        ThreadPool.QueueUserWorkItem(ThreadedSendData, networkPropValues);
    }

    private static void ThreadedSendData(object state)
    {
        var networkPropValues = (Dictionary<uint, Dictionary<PaletteSubId, Dictionary<BehaviourPropertyId, NetworkedPropertyChange>>>)state;
        var packets = new List<NetworkedPropertiesDataPacket>();
        
        foreach (var (networkId, subPropertyValues) in networkPropValues)
        {
            foreach (var (paletteSubId, propertyValues) in subPropertyValues)
            {
                var behaviourChanges = new Dictionary<uint, List<NetworkedPropertyChange>>();
                foreach (var (behaviourPropertyId, value) in propertyValues)
                {
                    var behaviourClassId = behaviourPropertyId.BehaviourClassId;

                    if (!behaviourChanges.TryGetValue(behaviourClassId, out var propChanges))
                        behaviourChanges[behaviourClassId] = propChanges = [];
                    
                    propChanges.Add(value);
                }

                var behaviourProps = new List<BehaviourPropertyChanges>();
                foreach (var (behaviourId, props) in behaviourChanges)
                {
                    var changes = new BehaviourPropertyChanges
                    {
                        BehaviourClassId = behaviourId,
                        GuidPaletteId = paletteSubId.GuidPaletteId,
                        SubId = paletteSubId.SubId,
                        PropertyChanges = props.ToArray(),
                    };
                    
                    behaviourProps.Add(changes);
                }

                var packet = new NetworkedPropertiesDataPacket
                {
                    NetworkId = networkId,
                    BehaviourProperties = behaviourProps.ToArray(),
                };
                packets.Add(packet);
            }
        }
        
        RepoSteamNetwork.SendPackets(packets);
    }
}