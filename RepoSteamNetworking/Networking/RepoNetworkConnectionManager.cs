using System;
using System.Runtime.InteropServices;
using RepoSteamNetworking.API;
using RepoSteamNetworking.Networking.Packets;
using RepoSteamNetworking.Utils;
using Steamworks;
using Steamworks.Data;

namespace RepoSteamNetworking.Networking;

internal class RepoNetworkConnectionManager : ConnectionManager
{
    private Lobby _currentLobby;
    
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
        
        RepoSteamNetwork.OnClientMessageReceived(bytes);
    }

    public void SetLobby(Lobby lobby)
    {
        _currentLobby = lobby;
    }

    // Send handshake packet to host to verify this connection is from a player who is actually in the lobby, and not 
    // a potentially malicious user attempting to connect from outside the lobby.
    public void StartHandshake()
    {
        Logging.Info("Sending handshake to server to verify connection!");
        
        var handshakePacket = new InitialHandshakePacket();
        handshakePacket.SetData(_currentLobby);
        RepoSteamNetwork.SendPacket(handshakePacket, NetworkDestination.HostOnly);
    }
}