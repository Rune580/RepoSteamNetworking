using System;
using System.Reflection;
using RepoSteamNetworking.Networking;
using RepoSteamNetworking.Networking.Data;
using RepoSteamNetworking.Networking.Packets;
using RepoSteamNetworking.Utils;
using RepoSteamNetworking.Utils.Reflection;
using Steamworks;
using Steamworks.Data;

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
        var assembly = Assembly.GetCallingAssembly();
        
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
}