using RepoSteamNetworking.API.Unity;
using RepoSteamNetworking.Networking.Data;
using RepoSteamNetworking.Networking.Packets;
using RepoSteamNetworking.Networking.Unity;

namespace RepoSteamNetworking.Networking.NetworkedProperties;

public static class NetworkedPropertyManager
{
    private static readonly NetworkedPropertyQueue PropertyQueue = new();
    
    public static void AddNetworkedPropertyDataToQueue(object value, uint networkId, string modGuid, uint subId, string className, uint propId, byte changeKind)
    {
        var guidPaletteId = RepoSteamNetworkManager.Instance.GetGuidPaletteId(modGuid);
        var paletteSubId = new PaletteSubId(guidPaletteId, subId);
        
        var behaviourId = RepoSteamNetworkManager.Instance.GetBehaviourId(className);
        var behaviourPropId = new BehaviourPropertyId(behaviourId, propId);

        var propertyChange = new NetworkedPropertyChange
        {
            ChangeKind = (VariableChangeKind)changeKind,
            PropertyId = propId,
            Value = value,
        };
        
        PropertyQueue[networkId, paletteSubId, behaviourPropId] = propertyChange;
    }

    internal static void SyncNetworkedProperties() => PropertyQueue.SendData();

    internal static void OnNetworkedPropertiesPacketReceived(NetworkedPropertiesDataPacket packet)
    {
        var networkManager = RepoSteamNetworkManager.Instance;
        var identity = networkManager.GetNetworkIdentity(packet.NetworkId);
        
        foreach (var behaviourChangeList in packet.BehaviourProperties)
        {
            var behaviourId = behaviourChangeList.BehaviourClassId;
            var className = networkManager.GetBehaviourClassName(behaviourId);
            
            var modGuid = networkManager.GetModGuid(behaviourChangeList.GuidPaletteId);
            var subIdentity = identity.GetSubIdentity(modGuid, behaviourChangeList.SubId);

            if (subIdentity is not INetworkedPropertyListener listener)
                continue;
            
            listener.OnNetworkedPropertiesDataReceived(className, behaviourChangeList.PropertyChanges);
        }
    }
}