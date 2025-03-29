using RepoSteamNetworking.Networking.Unity;

namespace RepoSteamNetworking.Networking.NetworkedProperties;

public static class NetworkedPropertyManager
{
    private static readonly NetworkedPropertyQueue PropertyQueue = new();
    
    public static void AddNetworkedPropertyDataToQueue(object value, uint networkId, string modGuid, uint subId, string className, uint propId)
    {
        var guidPaletteId = RepoSteamNetworkManager.Instance.GetGuidPaletteId(modGuid);
        
        // TODO: Possibility of collisions with this.
        // TODO: Generate Id's for classes with networked properties, palette-ize them, sync with clients, and use those Id's instead.
        ulong classId;
        unchecked
        {
            classId = (uint)className.GetHashCode();
        }
        
        // Pack classId and propId into an ulong for easier indexing.
        var packedPropId = classId << 32 | propId;
        
        PropertyQueue[networkId, subId, packedPropId] = value;
    }
}