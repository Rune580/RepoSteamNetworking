using System;
using System.Collections.Generic;
using System.Linq;
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
    private readonly Dictionary<ulong, uint> _steamIdConnectionLut = new();
    
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
        StopVerificationTimer(connection.Id);

        SteamId steamIdToRemove = default;
        foreach (var (steamId, connectionId) in _steamIdConnectionLut)
        {
            if (connection.Id != connectionId)
                continue;
            
            steamIdToRemove = steamId;
            break;
        }

        if (steamIdToRemove.IsValid)
            _steamIdConnectionLut.Remove(steamIdToRemove);
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
            var header = message.ReadPacketHeader();
            
            var packet = NetworkPacketRegistry.CreatePacket(header.PacketId);

            if (packet is not InitialHandshakePacket handshakePacket || header.Destination != NetworkDestination.HostOnly)
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
                
                _steamIdConnectionLut[handshakePacket.PlayerId] = connection.Id;
                
                Logging.Info($"Handshake successful! Connection {connection.Id} is now verified!");

                SendHandshakeStatus(header.Sender, true);
                
                return;
            }
            
            Logging.Warn($"Handshake failed!\n\tConnection sent: {handshakePacket.DebugFormat()}\n\tExpected: ( LobbyId: {RepoNetworkingServer.Instance.CurrentLobby.Id}, AuthKey: {RepoNetworkingServer.Instance.AuthKey} )");
            
            SendHandshakeStatus(header.Sender, false);
        }
        
        RepoSteamNetwork.OnHostReceivedMessage(bytes);
    }

    private void SendHandshakeStatus(SteamId target, bool success)
    {
        var successPacket = new HandshakeStatusPacket
        {
            Success = success
        };
        successPacket.SetTarget(target);
        RepoSteamNetwork.SendPacket(successPacket, NetworkDestination.PacketTarget);
    }

    public bool TryGetConnectionBySteamId(SteamId steamId, out Connection connection)
    {
        connection = default;

        if (!_steamIdConnectionLut.TryGetValue(steamId, out var connectionId))
            return false;

        connection = Connected.FirstOrDefault(conn => conn.Id == connectionId);

        return connection != 0;
    }

    public void Reset()
    {
        _verifiedConnectionIds.Clear();
        _steamIdConnectionLut.Clear();

        foreach (var (_, timer) in _verificationTimers)
            timer.Dispose();
        
        _verificationTimers.Clear();
    }
}