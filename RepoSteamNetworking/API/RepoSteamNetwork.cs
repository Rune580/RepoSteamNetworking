using System;
using RepoSteamNetworking.Networking;
using RepoSteamNetworking.Networking.Data;
using RepoSteamNetworking.Networking.Packets;

namespace RepoSteamNetworking.API;

public static class RepoSteamNetwork
{
    internal static void OnHostReceivedMessage(byte[] data)
    {
        var message = new SocketMessage(data);
        var packetId = message.Read<int>();
        var destination = (NetworkDestination)message.Read<byte>();
        
        var packet = NetworkPacketRegistry.CreatePacket(packetId);

        if (destination == NetworkDestination.HostOnly)
        {
            packet.Deserialize(message);
            packet.InvokeCallback();
            return;
        }

        RepoNetworkingServer.Instance.SendSocketMessageToClients(new SocketMessage(data));
    }
    
    internal static void OnClientMessageReceived(byte[] data)
    {
        var message = new SocketMessage(data);
        var packetId = message.Read<int>();
        var destination = (NetworkDestination)message.Read<byte>();

        var packet = NetworkPacketRegistry.CreatePacket(packetId);

        // Don't process packets that are for clients only on the host.
        if (destination == NetworkDestination.ClientsOnly && RepoNetworkingServer.Instance.ServerActive)
            return;
        
        packet.Deserialize(message);
        packet.InvokeCallback();
    }

    public static void RegisterPacket<TPacket>()
        where TPacket : NetworkPacket<TPacket>
    {
        NetworkPacketRegistry.RegisterPacket(typeof(TPacket));
    }

    public static void RegisterCallback<TPacket>(Action<TPacket> callback)
        where TPacket : NetworkPacket<TPacket>, new()
    {
        var packet = new TPacket();
        packet.RegisterCallback(callback);
    }

    public static void SendPacket<TPacket>(TPacket packet, NetworkDestination destination = NetworkDestination.Everyone)
        where TPacket : NetworkPacket
    {
        var message = packet.Serialize(destination);

        if (destination == NetworkDestination.HostOnly)
        {
            RepoNetworkingClient.Instance.SendSocketMessageToServer(message);
        }
        else if (RepoNetworkingServer.Instance.ServerActive)
        {
            RepoNetworkingServer.Instance.SendSocketMessageToClients(message);
        }
        else
        {
            RepoNetworkingClient.Instance.SendSocketMessageToServer(message);
        }
    }
}