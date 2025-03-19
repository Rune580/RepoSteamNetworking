using System;
using System.Linq;
using System.Timers;
using RepoSteamNetworking.API;
using RepoSteamNetworking.Networking.Packets;
using RepoSteamNetworking.Networking.Unity;
using RepoSteamNetworking.Utils;
using Steamworks;
using Steamworks.Data;

namespace RepoSteamNetworking.Networking;

internal class SteamUserConnection : IEquatable<SteamUserConnection>
{
    private string _clientKey = "";
    private Connection _connection;
    private readonly Timer _timer;

    public uint ConnectionId => _connection.Id;

    public SteamId SteamId { get; private set; }
    
    public ConnectionStatus Status { get; private set; }


    public string UserName
    {
        get
        {
            if (string.IsNullOrEmpty(field))
                field = SteamId.GetLobbyName();

            return field;
        }
    } = "";

    public SteamUserConnection(Connection connection)
    {
        _connection = connection;
        _timer = new Timer(10000);
        _timer.AutoReset = false;

        Status = ConnectionStatus.Unverified;
    }

    public void StartVerification()
    {
        _clientKey = RepoNetworkingServer.Instance.CreateAuthKey();

        var packet = new HandshakeStartAuthPacket
        {
            ClientKey = _clientKey
        };

        packet.Header.Sender = RepoSteamNetwork.CurrentSteamId;
        var message = packet.Serialize(NetworkDestination.Everyone); // Destination doesn't matter, the packet will be intercepted before it's processed.

        _connection.SendMessage(message.GetBytes());
            
        _timer.Elapsed += OnHandshakeTimeout;
        _timer.Start();
        
        Logging.Info($"Waiting for Connection {ConnectionId} to verify...");
    }

    public void StartModListValidation()
    {
        _timer.Elapsed += OnValidationTimeout;
        _timer.Start();
        
        Logging.Info($"Waiting for Client {UserName} to validate mod list compatibility...");
    }

    public bool VerifyAuth(HandshakeAuthConnectionPacket packet)
    {
        var lobby = RepoNetworkingServer.Instance.CurrentLobby;

        var authKey = lobby.GetMemberData(lobby.Owner, _clientKey);
        
        if (string.IsNullOrWhiteSpace(authKey) || authKey != packet.AuthKey)
            return false;
        
        if (lobby.Id != packet.LobbyId)
            return false;

        if (lobby.Members.Any(member => member.Id == packet.PlayerId))
            return true;
        
        return false;
    }

    public void SetVerifiedWithSteamId(SteamId steamId)
    {
        SteamId = steamId;
        Status = ConnectionStatus.Verified;
        
        Logging.Info($"Connection {ConnectionId} is now verified as Client {UserName}!");
        
        RepoNetworkingServer.Instance.RemoveAuthKey(_clientKey);

        _timer.Elapsed -= OnHandshakeTimeout;
        _timer.Stop();
    }

    public void SetValidated()
    {
        if (Status is not ConnectionStatus.Verified)
            return;

        _timer.Elapsed -= OnValidationTimeout;
        _timer.Stop();
        _timer.Dispose();

        Status = ConnectionStatus.VerifiedAndValid;
    }

    private void OnHandshakeTimeout(object sender, ElapsedEventArgs e)
    {
        _timer.Elapsed -= OnHandshakeTimeout;
        _timer.Stop();
        
        Logging.Warn($"Connection {ConnectionId} failed to verify in time, dropping connection...");
        
        RepoNetworkingServer.Instance.RemoveAuthKey(_clientKey);
        
        DropConnection();
    }

    private void OnValidationTimeout(object sender, ElapsedEventArgs e)
    {
        _timer.Elapsed -= OnValidationTimeout;
        _timer.Stop();
        
        Logging.Warn($"Client {UserName} failed to validate mod list compatibility in time, dropping connection...");
        
        DropConnection();
    }

    private void DropConnection()
    {
        _timer.Dispose();
        Close();
    }

    public void Close()
    {
        if (_timer.Enabled)
        {
            _timer.Stop();
            _timer.Dispose();
        }

        try
        {
            _connection.Close();
        }
        catch
        {
            // Catch all while trying to close.
        }
        
        Status = ConnectionStatus.Closed;
    }

    public Result SendMessage(byte[] data, SendType sendType = SendType.Reliable) =>
        _connection.SendMessage(data, sendType);

    public bool Equals(SteamUserConnection? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return ConnectionId.Equals(other.ConnectionId);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != GetType())
            return false;
        return Equals((SteamUserConnection)obj);
    }

    public override int GetHashCode()
    {
        return ConnectionId.GetHashCode();
    }

    public static bool operator ==(SteamUserConnection? left, SteamUserConnection? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(SteamUserConnection? left, SteamUserConnection? right)
    {
        return !Equals(left, right);
    }

    public enum ConnectionStatus
    {
        Unverified,
        Verified,
        VerifiedAndValid,
        Closed,
    }
}