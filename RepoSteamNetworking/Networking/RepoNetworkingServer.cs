using System;
using System.Linq;
using RepoSteamNetworking.Networking.Data;
using RepoSteamNetworking.Networking.Packets;
using RepoSteamNetworking.Utils;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

namespace RepoSteamNetworking.Networking;

public class RepoNetworkingServer : MonoBehaviour
{
    private static RepoNetworkingServer? _instance;
    public static RepoNetworkingServer Instance => _instance;

    private bool _serverActive;
    internal Lobby CurrentLobby { get; private set; }
    private RepoNetworkSocketManager? _socketManager;
    
    internal bool ServerActive => _instance is not null && _serverActive;
    
    internal string AuthKey => CurrentLobby.GetData("RSN_Auth_Key");
    
    internal static void CreateSingleton(GameObject parent)
    {
        if (_instance is not null)
        {
            Logging.Warn($"{nameof(RepoNetworkingServer)} already initialized!");
            return;
        }
        
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
        if (!_serverActive || _socketManager is null)
            return;
        
        // Just keep receiving data in an update loop?
        _socketManager.Receive();
    }

    public void StartSocketServer(Lobby lobby)
    {
        CurrentLobby = lobby;
        _socketManager = SteamNetworkingSockets.CreateRelaySocket<RepoNetworkSocketManager>();
        _serverActive = true;

        // Create a unique key and store it in the lobby, this requires clients to be a member of the lobby in order to access the key.
        var guid = Guid.NewGuid();
        CurrentLobby.SetData("RSN_Auth_Key", guid.ToString());
    }

    public void StopSocketServer()
    {
        _socketManager?.Close();
        _serverActive = false;
    }

    public void SendSocketMessageToClients(SocketMessage message)
    {
        if (_socketManager is null)
        {
            Logging.Info("No server to send messages, can't send message!");
            return;
        }
        
        var connected = _socketManager.Connected;
        foreach (var connection in connected)
        {
            var result = connection.SendMessage(message.GetBytes());
        }
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