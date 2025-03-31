using RepoSteamNetworking.Networking.Registries;
using RepoSteamNetworking.Networking.Serialization;
using RepoSteamNetworking.Utils;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

namespace RepoSteamNetworking.Networking.Unity;

public class RepoNetworkingClient : MonoBehaviour
{
    private static RepoNetworkingClient? _instance;
    public static RepoNetworkingClient Instance => _instance;
    
    private bool _clientActive;
    internal Lobby CurrentLobby { get; private set; }
    private RepoNetworkConnectionManager? _connectionManager;
    
    internal static void CreateSingleton(GameObject parent)
    {
        if (_instance is not null)
        {
            Logging.Warn($"{nameof(RepoNetworkingClient)} already initialized!");
            return;
        }
        
        VersionCompatRegistry.InitRegistry();
        ModNetworkGuidRegistry.Init();
        BehaviourIdRegistry.Init();
        
        Instantiate(new GameObject("RepoNetworkingClient"), parent.transform)
            .AddComponent<RepoNetworkingClient>();
        
        Logging.Info("Created RepoNetworkingClient");
    }

    private void Awake()
    {
        _instance = this;
        
        DontDestroyOnLoad(this);
    }

    private void Update()
    {
        if (!_clientActive || _connectionManager is null)
            return;
        
        // Just keep receiving data in an update loop?
        _connectionManager.Receive();
    }

    public void ConnectToServer(Lobby lobby)
    {
        CurrentLobby = lobby;
        _connectionManager = SteamNetworkingSockets.ConnectRelay<RepoNetworkConnectionManager>(lobby.Owner.Id);
        _clientActive = true;
        
        _connectionManager.SetLobby(lobby);
    }

    public void DisconnectFromServer()
    {
        _connectionManager?.Close();
        _clientActive = false;
    }

    public void SendSocketMessageToServer(SocketMessage message)
    {
        if (_connectionManager is null)
        {
            Logging.Info("No connection to server, can't send message!");
            return;
        }

        var result = _connectionManager.Connection.SendMessage(message.GetBytes());
    }

    internal void SetConnectionVerified(bool verified)
    {
        if (_connectionManager is null)
            return;
        
        _connectionManager.Verified = verified;
    }
}