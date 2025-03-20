using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using RepoSteamNetworking.API;
using RepoSteamNetworking.API.Asset;
using RepoSteamNetworking.API.VersionCompat;
using RepoSteamNetworking.Networking;
using RepoSteamNetworking.Networking.Packets;
using RepoSteamNetworking.Utils;
using UnityEngine;

namespace RepoSteamNetworking;

[RSNVersionCompatibility(VersionCompatibility.Any, optional: false)]
[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class RepoSteamNetworkingPlugin : BaseUnityPlugin
{
    internal static AssetBundleReference TestBundle;
    
    private void Awake()
    {
        Logging.SetLogSource(Logger);
        
        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), PluginInfo.PLUGIN_GUID);
        
        RegisterPackets();
        
        DoAssetBundleStuff();
    }

    private void RegisterPackets()
    {
        RepoSteamNetwork.RegisterPacket<HandshakeStartAuthPacket>();
        
        RepoSteamNetwork.RegisterPacket<HandshakeAuthConnectionPacket>();
        
        RepoSteamNetwork.RegisterPacket<HandshakeStatusPacket>();
        RepoSteamNetwork.AddCallback<HandshakeStatusPacket>(PacketHandler.OnHandshakeStatusReceived);
        
        RepoSteamNetwork.RegisterPacket<ClientModVersionRegistryPacket>();
        RepoSteamNetwork.AddCallback<ClientModVersionRegistryPacket>(PacketHandler.OnClientModVersionRegistryReceived);
        
        RepoSteamNetwork.RegisterPacket<ServerModVersionRegistryStatusPacket>();
        RepoSteamNetwork.AddCallback<ServerModVersionRegistryStatusPacket>(PacketHandler.OnServerModVersionRegistryReceived);
        
        RepoSteamNetwork.RegisterPacket<CallRPCPacket>();
        RepoSteamNetwork.AddCallback<CallRPCPacket>(PacketHandler.OnCallRPCPacketReceived);

        RepoSteamNetwork.RegisterPacket<InstantiateNetworkedPrefabServerPacket>();
        RepoSteamNetwork.AddCallback<InstantiateNetworkedPrefabServerPacket>(PacketHandler.OnInstantiateNetworkedPrefabServerPacketReceived);
        
        RepoSteamNetwork.RegisterPacket<InstantiateNetworkedPrefabClientPacket>();
        RepoSteamNetwork.AddCallback<InstantiateNetworkedPrefabClientPacket>(PacketHandler.OnInstantiateNetworkedPrefabClientPacketReceived);
    }

    private void DoAssetBundleStuff()
    {
        if (!Chainloader.PluginInfos.TryGetValue(PluginInfo.PLUGIN_GUID, out var pluginInfo))
            throw new Exception("Failed to find plugin info!");

        if (pluginInfo is null)
            throw new Exception("Plugin info is null!");
        
        var dllLoc = pluginInfo.Location;
        var parentDir = Directory.GetParent(dllLoc);
        
        if (parentDir is null)
            throw new Exception("Failed to find parent directory!");
        
        var assetBundlesDir = Path.Combine(parentDir.FullName, "AssetBundles");
        if (!Directory.Exists(assetBundlesDir))
            throw new Exception("AssetBundles directory doesn't exist!");
        
        var assetBundlePath = Path.Combine(assetBundlesDir, "steam-networking-testing-bundle");

        var assetBundle = AssetBundle.LoadFromFile(assetBundlePath);

        TestBundle = RepoSteamNetwork.RegisterAssetBundle(assetBundle, PluginInfo.PLUGIN_GUID);
    }
}