﻿using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using RepoSteamNetworking.API;
using RepoSteamNetworking.API.Asset;
using RepoSteamNetworking.API.VersionCompat;
using RepoSteamNetworking.Networking;
using RepoSteamNetworking.Networking.NetworkedProperties;
using RepoSteamNetworking.Networking.Packets;
using RepoSteamNetworking.Utils;
using UnityEngine;

namespace RepoSteamNetworking;

[RSNVersionCompatibility(VersionCompatibility.Any, optional: false)]
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class RepoSteamNetworkingPlugin : BaseUnityPlugin
{
    internal static AssetBundleReference TestBundle;
    
    private void Awake()
    {
        Logging.SetLogSource(Logger);
        
        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), MyPluginInfo.PLUGIN_GUID);
        
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
        
        RepoSteamNetwork.RegisterPacket<SetConnectionPalettesPacket>();
        RepoSteamNetwork.AddCallback<SetConnectionPalettesPacket>(PacketHandler.OnSetConnectionPalettesReceived);
        
        RepoSteamNetwork.RegisterPacket<CallRPCPacket>();
        RepoSteamNetwork.AddCallback<CallRPCPacket>(PacketHandler.OnCallRPCPacketReceived);
        
        RepoSteamNetwork.RegisterPacket<NetworkedPropertiesDataPacket>();
        RepoSteamNetwork.AddCallback<NetworkedPropertiesDataPacket>(NetworkedPropertyManager.OnNetworkedPropertiesPacketReceived);

        RepoSteamNetwork.RegisterPacket<InstantiateNetworkedPrefabServerPacket>();
        RepoSteamNetwork.AddCallback<InstantiateNetworkedPrefabServerPacket>(PacketHandler.OnInstantiateNetworkedPrefabServerPacketReceived);
        
        RepoSteamNetwork.RegisterPacket<InstantiateNetworkedPrefabClientPacket>();
        RepoSteamNetwork.AddCallback<InstantiateNetworkedPrefabClientPacket>(PacketHandler.OnInstantiateNetworkedPrefabClientPacketReceived);
    }

    private void DoAssetBundleStuff()
    {
        if (!Chainloader.PluginInfos.TryGetValue(MyPluginInfo.PLUGIN_GUID, out var pluginInfo))
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

        TestBundle = RepoSteamNetwork.RegisterAssetBundle(assetBundle, MyPluginInfo.PLUGIN_GUID);
    }
}