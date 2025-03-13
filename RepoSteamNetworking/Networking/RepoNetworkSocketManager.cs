using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using RepoSteamNetworking.API;
using RepoSteamNetworking.Networking.Data;
using RepoSteamNetworking.Networking.Packets;
using RepoSteamNetworking.Utils;
using Steamworks;
using Steamworks.Data;

namespace RepoSteamNetworking.Networking;

internal class RepoNetworkSocketManager : SocketManager
{
    private readonly HashSet<uint> _verifiedConnectionIds = [];
    private readonly Dictionary<uint, Timer> _verificationTimers = new();
    
    public Action? OnClientConnected;
    
    public RepoNetworkSocketManager()
    {
        Logging.Info("Creating new SocketManager");
    }
    
    public override void OnConnecting(Connection connection, ConnectionInfo info)
    {
        base.OnConnecting(connection, info);
        
        Logging.Info($"{info.Identity} is connecting...");

        connection.Accept();
    }

    public override void OnConnected(Connection connection, ConnectionInfo info)
    {
        base.OnConnected(connection, info);
        
        Logging.Info($"{info.Identity} has connected!");
        
        var handshakeTimer = new Timer(DropUnverifiedConnection, connection, TimeSpan.FromSeconds(10), TimeSpan.FromMilliseconds(-1));
        _verificationTimers[connection.Id] = handshakeTimer;
        
        OnClientConnected?.Invoke();
    }

    private void DropUnverifiedConnection(object state)
    {
        var connection = (Connection)state;
        var id = connection.Id;
        
        Logging.Warn($"Connection {id} failed to verify, dropping connection...");

        StopVerificationTimer(id);

        connection.Close();
    }

    private void StopVerificationTimer(uint connectionId)
    {
        if (!_verificationTimers.TryGetValue(connectionId, out var timer))
        {
            Logging.Debug($"No verification timer for {connectionId}!");
            return;
        }
        
        timer.Dispose();
        _verificationTimers.Remove(connectionId);
    }

    public override void OnDisconnected(Connection connection, ConnectionInfo info)
    {
        base.OnDisconnected(connection, info);
        
        Logging.Info($"{info.Identity} has disconnected!");

        _verifiedConnectionIds.Remove(connection.Id);
    }

    public override void OnMessage(Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel)
    {
        base.OnMessage(connection, identity, data, size, messageNum, recvTime, channel);
        
        var bytes = new byte[size];
        Marshal.Copy(data, bytes, 0, size);
        
        // Message came from unverified connection, Check to see if it's a handshake packet
        if (!_verifiedConnectionIds.Contains(connection.Id))
        {
            var message = new SocketMessage(bytes);
            var packetId = message.Read<int>();
            var destination = (NetworkDestination)message.Read<byte>();
            
            var packet = NetworkPacketRegistry.CreatePacket(packetId);

            if (packet is not InitialHandshakePacket handshakePacket || destination != NetworkDestination.HostOnly)
            {
                Logging.Warn($"Received {packet.GetType()} packet from an unverified connection! dropping packet...");
                return;
            }

            handshakePacket.Deserialize(message);
            
            Logging.Info(handshakePacket.DebugFormat(true));
            
            if (RepoNetworkingServer.Instance.VerifyHandshake(handshakePacket))
            {
                _verifiedConnectionIds.Add(connection.Id);
                StopVerificationTimer(connection.Id);
                
                Logging.Info($"Handshake successful! Connection {connection.Id} is now verified!");
                
                return;
            }
            
            Logging.Warn($"Handshake failed!\n\tConnection sent: {handshakePacket.DebugFormat()}\n\tExpected: ( LobbyId: {RepoNetworkingServer.Instance.CurrentLobby.Id}, AuthKey: {RepoNetworkingServer.Instance.AuthKey} )");
        }
        
        RepoSteamNetwork.OnHostReceivedMessage(bytes);
    }
}