using System.Collections.Generic;
using RepoSteamNetworking.API;
using RepoSteamNetworking.Networking.Data;

namespace RepoSteamNetworking.Networking.Packets;

internal class ClientModVersionRegistryPacket : NetworkPacket<ClientModVersionRegistryPacket>
{
    public Dictionary<string, System.Version> ModVersions { get; set; } = new();
    
    protected override void WriteData(SocketMessage socketMessage)
    {
        socketMessage.Write(ModVersions);
    }

    protected override void ReadData(SocketMessage socketMessage)
    {
        ModVersions = socketMessage.Read<Dictionary<string, System.Version>>();
    }
}