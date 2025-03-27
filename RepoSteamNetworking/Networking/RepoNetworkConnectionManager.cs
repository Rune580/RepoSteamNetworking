using System;
using System.Runtime.InteropServices;
using RepoSteamNetworking.API;
using RepoSteamNetworking.Networking.Data;
using RepoSteamNetworking.Networking.Packets;
using RepoSteamNetworking.Networking.Registries;
using RepoSteamNetworking.Utils;
using Steamworks;
using Steamworks.Data;

namespace RepoSteamNetworking.Networking;

internal class RepoNetworkConnectionManager : ConnectionManager
{
    private Lobby _currentLobby;
    internal bool Verified;
    
    public RepoNetworkConnectionManager()
    {
        Logging.Info("Creating new ConnectionManager");
    }
    
    public override void OnConnectionChanged(ConnectionInfo info)
    {
        base.OnConnectionChanged(info);
    }

    public override void OnConnecting(ConnectionInfo info)
    {
        base.OnConnecting(info);
    }

    public override void OnConnected(ConnectionInfo info)
    {
        base.OnConnected(info);
        
        Logging.Info($"Connected to server!");
    }

    public override void OnDisconnected(ConnectionInfo info)
    {
        base.OnDisconnected(info);
        
        Logging.Info($"Disconnected from server!");
    }

    public override void OnMessage(IntPtr data, int size, long messageNum, long recvTime, int channel)
    {
        base.OnMessage(data, size, messageNum, recvTime, channel);
        
        var bytes = new byte[size];
        Marshal.Copy(data, bytes, 0, size);

        if (!Verified)
        {
            var message = new SocketMessage(bytes);
            var header = message.ReadPacketHeader();
            
            var packet = NetworkPacketRegistry.CreatePacket(header.PacketId);

            if (packet is HandshakeStatusPacket)
            {
                RepoSteamNetwork.OnClientMessageReceived(bytes);
                return;
            }

            if (packet is not HandshakeStartAuthPacket startAuthPacket)
            {
                Logging.Warn($"Expected {nameof(HandshakeStartAuthPacket)} from Server but got {packet.GetType()} instead!");
                return;
            }

            startAuthPacket.Deserialize(message);
            
            Logging.Info("Sending handshake to server to verify connection!");
            
            var handshakePacket = new HandshakeAuthConnectionPacket();
            handshakePacket.SetData(_currentLobby, startAuthPacket.ClientKey);
            RepoSteamNetwork.SendPacket(handshakePacket, NetworkDestination.HostOnly);
            
            return;
        }
        
        RepoSteamNetwork.OnClientMessageReceived(bytes);
    }

    public void SetLobby(Lobby lobby)
    {
        _currentLobby = lobby;
    }
}