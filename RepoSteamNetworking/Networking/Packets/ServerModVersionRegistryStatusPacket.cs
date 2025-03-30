using System.Collections.Generic;
using RepoSteamNetworking.API;
using RepoSteamNetworking.API.VersionCompat;
using RepoSteamNetworking.Networking.Serialization;

namespace RepoSteamNetworking.Networking.Packets;

internal class ServerModVersionRegistryStatusPacket : NetworkPacket<ServerModVersionRegistryStatusPacket>
{
    private readonly List<ServerModRegistryEntry> _registryEntries = [];

    public ServerModRegistryEntry[] ModIncompatibilities { get; private set; } = [];
    
    public void AddIncompatible(string modGuid, System.Version serverModVersion, VersionCompatibility compatibility)
    {
        _registryEntries.Add(new ServerModRegistryEntry
        {
            Guid = modGuid,
            Version = serverModVersion,
            Compatibility = compatibility
        });
    }
    
    public bool HasIncompatibilities() => _registryEntries.Count > 0;
    
    protected override void WriteData(SocketMessage socketMessage)
    {
        socketMessage.Write(_registryEntries.ToArray());
    }

    protected override void ReadData(SocketMessage socketMessage)
    {
        ModIncompatibilities = socketMessage.Read<ServerModRegistryEntry[]>();
    }

    internal struct ServerModRegistryEntry
    {
        public string Guid;
        public System.Version Version;
        public VersionCompatibility Compatibility;
    }
}