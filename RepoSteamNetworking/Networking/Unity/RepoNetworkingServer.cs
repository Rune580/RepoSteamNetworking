using System;
using RepoSteamNetworking.Networking.Data;
using RepoSteamNetworking.Networking.Registries;
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
    
    // internal string AuthKey => CurrentLobby.GetMemberData(CurrentLobby.Owner, "RSN_Auth_Key");
    
    internal static void CreateSingleton(GameObject parent)
    {
        if (_instance is not null)
        {
            Logging.Warn($"{nameof(RepoNetworkingServer)} already initialized!");
            return;
        }
        
        VersionCompatRegistry.InitRegistry();
        ModNetworkGuidRegistry.Init();
        
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
        
        SocketManager.Receive();
    }

    public void StartSocketServer(Lobby lobby)
    {
        CurrentLobby = lobby;
        SocketManager = SteamNetworkingSockets.CreateRelaySocket<RepoNetworkSocketManager>();
        _serverActive = true;
        
        RepoSteamNetworkManager.Instance.SetModGuidPalette(ModNetworkGuidRegistry.Palette);
    }

    public string CreateAuthKey()
    {
        // Create a unique key and store it in the lobby, this requires clients to be a member of the lobby in order to access the key.
        var serverAuthKey = Guid.NewGuid().ToString();

        var clientKey = $"RSN_{Guid.NewGuid().ToString()}";
        CurrentLobby.SetMemberData(clientKey, serverAuthKey);

        return clientKey;
    }

    public void RemoveAuthKey(string clientKey) => CurrentLobby.SetMemberData(clientKey, string.Empty);

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
            if (connection.State is not ConnectionState.Connected)
                continue;
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

        if (connection.State is not ConnectionState.Connected)
        {
            Logging.Info($"Can't send message to unconnected client {target}");
            return;
        }
        
        var result = connection.SendMessage(message.GetBytes());
    }
}