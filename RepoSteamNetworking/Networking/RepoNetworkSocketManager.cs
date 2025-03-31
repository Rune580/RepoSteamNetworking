using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using RepoSteamNetworking.API;
using RepoSteamNetworking.Networking.Data;
using RepoSteamNetworking.Networking.Packets;
using RepoSteamNetworking.Networking.Registries;
using RepoSteamNetworking.Utils;
using Steamworks;
using Steamworks.Data;

namespace RepoSteamNetworking.Networking;

internal class RepoNetworkSocketManager : SocketManager
{
    private readonly List<SteamUserConnection> _steamUserConnections = [];
    private readonly Dictionary<uint, int> _connectionIdSteamUserConnectionLut = [];
    private readonly Dictionary<ulong, int> _steamIdSteamUserConnectionLut = [];

    public IReadOnlyCollection<SteamUserConnection> UserConnections => _steamUserConnections;
    
    public RepoNetworkSocketManager()
    {
        Logging.Info("Creating new SocketManager");
    }
    
    public override void OnConnecting(Connection connection, ConnectionInfo info)
    {
        base.OnConnecting(connection, info);

        connection.Accept();
    }

    public override void OnConnected(Connection connection, ConnectionInfo info)
    {
        base.OnConnected(connection, info);

        AddNewSteamUserConnection(connection, info);
    }

    private void AddNewSteamUserConnection(Connection connection, ConnectionInfo info)
    {
        var steamUserConnection = new SteamUserConnection(connection, info);
        var steamId = info.identity.SteamId.Value;

        var index = _steamUserConnections.Count;
        _connectionIdSteamUserConnectionLut[connection.Id] = index;
        _steamIdSteamUserConnectionLut[steamId] = index;

        _steamUserConnections.Add(steamUserConnection);
        
        steamUserConnection.StartVerification();
    }

    public bool TryGetSteamUserConnection(uint connectionId, out SteamUserConnection userConnection)
    {
        userConnection = null!;

        if (!_connectionIdSteamUserConnectionLut.TryGetValue(connectionId, out var i))
            return false;

        userConnection = _steamUserConnections[i];
        
        return userConnection is not null;
    }
    
    public bool TryGetSteamUserConnection(SteamId steamId, out SteamUserConnection userConnection)
    {
        userConnection = null!;

        if (!_steamIdSteamUserConnectionLut.TryGetValue(steamId.Value, out var i))
            return false;

        userConnection = _steamUserConnections[i];

        return userConnection is not null;
    }

    private void RemoveSteamUserConnection(SteamUserConnection userConnection)
    {
        _connectionIdSteamUserConnectionLut.Remove(userConnection.ConnectionId);
        _steamUserConnections.Remove(userConnection);
    }

    public override void OnDisconnected(Connection connection, ConnectionInfo info)
    {
        base.OnDisconnected(connection, info);

        if (!TryGetSteamUserConnection(connection.Id, out var userConnection))
        {
            Logging.Info($"Connection {connection.Id} has disconnected!");
            return;
        }

        if (userConnection.Status is not SteamUserConnection.ConnectionStatus.Verified and SteamUserConnection.ConnectionStatus.VerifiedAndValid)
        {
            Logging.Info($"Connection {connection.Id} has disconnected!");
        }
        else
        {
            Logging.Info($"Client {userConnection.UserName} has disconnected!");
        }

        RemoveSteamUserConnection(userConnection);
    }

    public override void OnMessage(Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel)
    {
        base.OnMessage(connection, identity, data, size, messageNum, recvTime, channel);
        
        var bytes = new byte[size];
        Marshal.Copy(data, bytes, 0, size);

        if (!TryGetSteamUserConnection(connection.Id, out var userConnection))
        {
            Logging.Warn("Received message from unknown connection");
            return;
        }
        
        // Message came from unverified connection, Check to see if it's a handshake packet
        if (userConnection.Status is SteamUserConnection.ConnectionStatus.Unverified)
        {
            var message = new SocketMessage(bytes);
            var header = message.ReadPacketHeader();
            
            var packet = NetworkPacketRegistry.CreatePacket(header.PacketId);

            if (packet is not HandshakeAuthConnectionPacket handshakePacket || header.Destination != NetworkDestination.HostOnly)
            {
                Logging.Warn($"Received {packet.GetType()} packet from an unverified connection {connection.Id}! dropping packet...");
                return;
            }

            handshakePacket.Deserialize(message);
            
            if (userConnection.VerifyAuth(handshakePacket))
            {
                userConnection.SetVerified();
                
                SendHandshakeStatus(identity.SteamId, true);
                userConnection.StartModListValidation();
                
                return;
            }
            
            Logging.Warn($"Handshake failed!");
            
            SendHandshakeStatus(identity.SteamId, false);
            return;
        }

        if (userConnection.Status is not SteamUserConnection.ConnectionStatus.VerifiedAndValid)
        {
            var message = new SocketMessage(bytes);
            var header = message.ReadPacketHeader();
            
            var packet = NetworkPacketRegistry.CreatePacket(header.PacketId);
            
            if (packet is not ClientModVersionRegistryPacket)
            {
                Logging.Warn($"Received {packet.GetType()} packet from Client {userConnection.UserName}! Expected mod list compatibility packet! dropping packet...");
                return;
            }
        }
        
        RepoSteamNetwork.OnHostReceivedMessage(bytes, connection, identity);
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

    public void Reset()
    {
        _connectionIdSteamUserConnectionLut.Clear();
        _steamIdSteamUserConnectionLut.Clear();

        foreach (var userConnection in _steamUserConnections)
            userConnection.Close();
        
        _steamUserConnections.Clear();
    }
}