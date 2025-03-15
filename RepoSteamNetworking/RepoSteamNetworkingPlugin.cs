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
        RepoSteamNetwork.RegisterCallback<HandshakeStatusPacket>(PacketHandler.OnHandshakeStatusReceived);
        
        RepoSteamNetwork.RegisterPacket<ClientModVersionRegistryPacket>();
        RepoSteamNetwork.RegisterCallback<ClientModVersionRegistryPacket>(PacketHandler.OnClientModVersionRegistryReceived);
        
        RepoSteamNetwork.RegisterPacket<ServerModVersionRegistryStatusPacket>();
        RepoSteamNetwork.RegisterCallback<ServerModVersionRegistryStatusPacket>(PacketHandler.OnServerModVersionRegistryReceived);
    }
}