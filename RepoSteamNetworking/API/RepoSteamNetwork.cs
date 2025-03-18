using System;
using System.Linq;
using System.Reflection;
using BepInEx;
using RepoSteamNetworking.API.Asset;
using RepoSteamNetworking.API.VersionCompat;
using RepoSteamNetworking.Networking;
using RepoSteamNetworking.Networking.Data;
using RepoSteamNetworking.Networking.Packets;
using RepoSteamNetworking.Networking.Unity;
using RepoSteamNetworking.Utils;
using RepoSteamNetworking.Utils.Reflection;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

namespace RepoSteamNetworking.API;

public static class RepoSteamNetwork
{
    public static SteamId CurrentSteamId
    {
        get
        {
            if (!field.IsValid)
                field = SteamClient.SteamId;
            
            return field;
        }
    }

    private static readonly RPCMethodHelper RPCHelper = new();

    internal static void OnHostReceivedMessage(byte[] data)
    {
        var message = new SocketMessage(data);
        var header = message.ReadPacketHeader();
        
        var packet = NetworkPacketRegistry.CreatePacket(header.PacketId);
        packet.Header = header;

        if (header.Destination == NetworkDestination.HostOnly)
        {
            packet.Deserialize(message);
            NetworkPacketRegistry.InvokeCallbacks(packet);
            return;
        }

        if (header.Destination == NetworkDestination.PacketTarget)
        {
            if (!header.Target.IsValid)
            {
                Logging.Warn("Invalid target specified for packet! Dropping packet...");
                return;
            }
            
            // Is the host the target of the packet?
            if (header.Target == CurrentSteamId)
            {
                packet.Deserialize(message);
                NetworkPacketRegistry.InvokeCallbacks(packet);
                return;
            }
        }

        RepoNetworkingServer.Instance.SendSocketMessageToClients(new SocketMessage(data));
    }
    
    internal static void OnClientMessageReceived(byte[] data)
    {
        var message = new SocketMessage(data);
        var header = message.ReadPacketHeader();

        var packet = NetworkPacketRegistry.CreatePacket(header.PacketId);
        packet.Header = header;

        // Don't process packets that are for clients only on the host.
        if (header.Destination == NetworkDestination.ClientsOnly && RepoNetworkingServer.Instance.ServerActive)
            return;
        
        packet.Deserialize(message);
        NetworkPacketRegistry.InvokeCallbacks(packet);
    }

    internal static Lobby GetCurrentLobby()
    {
        if (RepoNetworkingServer.Instance.ServerActive)
            return RepoNetworkingServer.Instance.CurrentLobby;

        return RepoNetworkingClient.Instance.CurrentLobby;
    }

    public static void RegisterPacket<TPacket>()
        where TPacket : NetworkPacket<TPacket>
    {
        NetworkPacketRegistry.RegisterPacket(typeof(TPacket));
    }

    public static void AddCallback<TPacket>(Action<TPacket> callback)
        where TPacket : NetworkPacket<TPacket>
    {
        var callbackHandler = MethodHandler.FromAction(callback);
        NetworkPacketRegistry.AddCallback<TPacket>(callbackHandler);
    }

    public static void RemoveCallback<TPacket>(Action<TPacket> callback)
        where TPacket : NetworkPacket<TPacket>
    {
        var callbackHandler = MethodHandler.FromAction(callback);
        NetworkPacketRegistry.RemoveCallback<TPacket>(callbackHandler);
    }

    public static void SetVersionCompatibility(string modGuid, System.Version modVersion, VersionCompatibility compatibility, bool optional = false)
    {
        VersionCompatRegistry.RegisterMod(modGuid, modVersion, new RSNVersionCompatibilityAttribute(compatibility, optional));
    }

    public static void SetVersionCompatibility(VersionCompatibility compatibility, bool optional = false, BaseUnityPlugin? plugin = null)
    {
        BepInPlugin pluginInfo;
        
        if (plugin is null)
        {
            var assembly = Assembly.GetCallingAssembly();

            var plugins = assembly.GetLoadableTypes()
                .SelectMany(type => type.GetCustomAttributes<BepInPlugin>())
                .ToArray();

            if (plugins.Length != 1)
                throw new InvalidOperationException($"Couldn't determine BepInPlugin from calling assembly! please set {nameof(plugin)} parameter.");
            
            pluginInfo = plugins[0];
        }
        else
        {
            pluginInfo = plugin.GetType().GetCustomAttribute<BepInPlugin>();
        }

        VersionCompatRegistry.RegisterMod(pluginInfo.GUID, pluginInfo.Version, new RSNVersionCompatibilityAttribute(compatibility, optional));
    }

    public static void SendPacket<TPacket>(TPacket packet, NetworkDestination destination = NetworkDestination.Everyone)
        where TPacket : NetworkPacket
    {
        packet.Header.Sender = CurrentSteamId;
        
        var message = packet.Serialize(destination);
        
        if (destination == NetworkDestination.HostOnly)
        {
            RepoNetworkingClient.Instance.SendSocketMessageToServer(message);
        }
        else if (RepoNetworkingServer.Instance.ServerActive)
        {
            if (destination == NetworkDestination.PacketTarget && packet.Header.Target.IsValid)
            {
                RepoNetworkingServer.Instance.SendSocketMessageToTarget(message, packet.Header.Target);
            }
            else
            {
                RepoNetworkingServer.Instance.SendSocketMessageToClients(message);
            }
        }
        else
        {
            RepoNetworkingClient.Instance.SendSocketMessageToServer(message);
        }
    }

    public static void CallRPC(uint networkId, uint subId, string methodName, params object[] parameters)
    {
        var packet = new CallRPCPacket
        {
            NetworkId = networkId,
            SubId = subId,
            MethodName = methodName,
            Parameters = parameters
        };
        
        SendPacket(packet);
    }

    internal static void InvokeRPC(uint networkId, uint subId, string methodName, params object[] parameters)
    {
        var networkIdentity = RepoSteamNetworkManager.Instance.GetNetworkIdentity(networkId);
        var subIdentity = networkIdentity.GetSubIdentity(subId);
        
        RPCHelper.InvokeRPC(subIdentity, methodName, parameters);
    }
    
    public static void InstantiatePrefab(AssetReference prefab) => InstantiatePrefab(prefab, Vector3.zero, Quaternion.identity);
    
    public static void InstantiatePrefab(AssetReference prefab, Quaternion rotation) => InstantiatePrefab(prefab, Vector3.zero, rotation);

    public static void InstantiatePrefab(AssetReference prefab, Vector3 position) => InstantiatePrefab(prefab, position, Quaternion.identity);
    
    public static void InstantiatePrefab(AssetReference prefab, Vector3 position, Quaternion rotation)
    {
        var packet = new InstantiateNetworkedPrefabServerPacket
        {
            Prefab = prefab,
            Position = position,
            Rotation = rotation
        };
        SendPacket(packet, NetworkDestination.HostOnly);
    }
}