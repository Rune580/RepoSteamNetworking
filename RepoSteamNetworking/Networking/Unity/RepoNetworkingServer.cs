using System;
using System.Linq;
using RepoSteamNetworking.Networking.Data;
using RepoSteamNetworking.Networking.Packets;
using RepoSteamNetworking.Utils;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

namespace RepoSteamNetworking.Networking.Unity;

public class RepoNetworkingServer : MonoBehaviour
{
    private static RepoNetworkingServer? _instance;
    public static RepoNetworkingServer Instance => _instance;

    private bool _serverActive;
    internal Lobby CurrentLobby { get; private set; }
    internal RepoNetworkSocketManager? SocketManager { get; private set; }
    
    internal bool ServerActive => _instance is not null && _serverActive;
    
    internal string AuthKey => CurrentLobby.GetMemberData(CurrentLobby.Owner, "RSN_Auth_Key");
    
    internal static void CreateSingleton(GameObject parent)
    {
        if (_instance is not null)
        {
            Logging.Warn($"{nameof(RepoNetworkingServer)} already initialized!");
            return;
        }
        
        VersionCompatRegistry.InitRegistry();
        
        Instantiate(new GameObject("RepoNetworkingServer"), parent.transform)
            .AddComponent<RepoNetworkingServer>();
        
        Logging.Info("Created RepoNetworkingServer");
    }
    
    private void Awake()
    {
        _instance = this;
        
        hideFlags = HideFlags.HideAndDontSave;
        DontDestroyOnLoad(this);
    }

    private void Update()
    {
        if (!_serverActive || SocketManager is null)
            return;
        
        // Just keep receiving data in an update loop?
        SocketManager.Receive();
    }

    public void StartSocketServer(Lobby lobby)
    {
        CurrentLobby = lobby;
        SocketManager = SteamNetworkingSockets.CreateRelaySocket<RepoNetworkSocketManager>();
        _serverActive = true;
        
        CreateAuthKeyForHandshake();
    }

    public void CreateAuthKeyForHandshake()
    {
        // Create a unique key and store it in the lobby, this requires clients to be a member of the lobby in order to access the key.
        var guid = Guid.NewGuid();
        CurrentLobby.SetMemberData("RSN_Auth_Key", guid.ToString());
    }

    public void StopSocketServer()
    {
        SocketManager?.Close();
        SocketManager?.Reset();
        _serverActive = false;
    }

    public void SendSocketMessageToClients(SocketMessage message)
    {
        if (SocketManager is null)
        {
            Logging.Info("No server to send messages, can't send message!");
            return;
        }
        
        var connected = SocketManager.UserConnections;
        foreach (var connection in connected)
        {
            var result = connection.SendMessage(message.GetBytes());
        }
    }

    public void SendSocketMessageToTarget(SocketMessage message, SteamId target)
    {
        if (SocketManager is null)
        {
            Logging.Info("No server to send messages, can't send message!");
            return;
        }
        
        if (!SocketManager.TryGetSteamUserConnection(target, out var connection))
        {
            Logging.Info($"No valid connection found for {target}!");
            return;
        }
        
        var result = connection.SendMessage(message.GetBytes());
    }

    internal bool VerifyHandshake(InitialHandshakePacket packet)
    {
        if (AuthKey != packet.AuthKey)
            return false;
        
        if (CurrentLobby.Id != packet.LobbyId)
            return false;

        if (CurrentLobby.Members.Any(member => member.Id == packet.PlayerId))
            return true;
        
        return false;
    }
}