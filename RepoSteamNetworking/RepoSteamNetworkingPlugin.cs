using System.Reflection;
using BepInEx;
using HarmonyLib;
using RepoSteamNetworking.API;
using RepoSteamNetworking.API.VersionCompat;
using RepoSteamNetworking.Networking;
using RepoSteamNetworking.Networking.Packets;
using RepoSteamNetworking.Utils;

namespace RepoSteamNetworking;

[RSNVersionCompatibility]
[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class RepoSteamNetworkingPlugin : BaseUnityPlugin
{
    private void Awake()
    {
        Logging.SetLogSource(Logger);
        
        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), PluginInfo.PLUGIN_GUID);
        
        RegisterPackets();
    }

    private void RegisterPackets()
    {
        RepoSteamNetwork.RegisterPacket<InitialHandshakePacket>();
        
        RepoSteamNetwork.RegisterPacket<HandshakeStatusPacket>();
        RepoSteamNetwork.AddCallback<HandshakeStatusPacket>(OnHandshakeStatusReceived);
        
        RepoSteamNetwork.RegisterPacket<ClientModVersionRegistryPacket>();
        RepoSteamNetwork.AddCallback<ClientModVersionRegistryPacket>(PacketHandler.OnClientModVersionRegistryReceived);
        
        RepoSteamNetwork.RegisterPacket<ServerModVersionRegistryStatusPacket>();
        RepoSteamNetwork.AddCallback<ServerModVersionRegistryStatusPacket>(PacketHandler.OnServerModVersionRegistryReceived);
    }
    
    private static void OnHandshakeStatusReceived(HandshakeStatusPacket packet)
    {
        if (!packet.Success)
        {
            Logging.Error("Failed to verify with server! Either something has gone terribly wrong, or steam is down!");
            return;
        }
        
        // Send server the version registry of the client to verify mod compatibility.
        
        Logging.Info("Sending mod compat registry to server to verify mod list compatibility.");
        var registryPacket = VersionCompatRegistry.CreateRegistryPacket();
        RepoSteamNetwork.SendPacket(registryPacket, NetworkDestination.HostOnly);
    }
}