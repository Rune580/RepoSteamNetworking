using System;
using RepoSteamNetworking.Networking;
using RepoSteamNetworking.Networking.Data;
using RepoSteamNetworking.Networking.Packets;
using RepoSteamNetworking.Utils;


namespace RepoSteamNetworking;

public static class RepoSteamNetwork
{
    internal static void OnHostReceivedMessage(byte[] data)
    {
        var message = new SocketMessage(data);
        var packetId = message.ReadInt();
        
        var destination = (NetworkDestination)message.ReadByte();
        
        var packet = NetworkPacketRegistry.CreatePacket(packetId);
        
        // packet.Deserialize(message);
        // packet.InvokeCallback(packet);

        if (destination == NetworkDestination.HostOnly)
        {
            packet.Deserialize(message);
            packet.InvokeCallback();
            return;
        }

        if (destination == NetworkDestination.Everyone)
        {
            RepoNetworkingServer.Instance.SendSocketMessageToClients(new SocketMessage(data));
        }
    }
    
    internal static void OnClientMessageReceived(byte[] data)
    {
        Logging.Info($"Received {data.Length} bytes from server!");
        
        var message = new SocketMessage(data);
        var packetId = message.ReadInt();

        var packet = NetworkPacketRegistry.CreatePacket(packetId);
        
        packet.Deserialize(message);
        packet.InvokeCallback();
    }

    public static void RegisterPacket<TPacket>()
        where TPacket : NetworkPacket<TPacket>, new()
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

        if (RepoNetworkingServer.Instance.ServerActive)
        {
            RepoNetworkingServer.Instance.SendSocketMessageToClients(message);
        }
        else
        {
            RepoNetworkingClient.Instance.SendSocketMessageToServer(message);
        }
    }
}