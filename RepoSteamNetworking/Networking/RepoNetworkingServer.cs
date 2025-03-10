using System.Text;
using RepoSteamNetworking.Networking.Data;
using RepoSteamNetworking.Utils;
using Steamworks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RepoSteamNetworking.Networking;

public class RepoNetworkingServer : MonoBehaviour
{
    private static RepoNetworkingServer? _instance;
    public static RepoNetworkingServer Instance => _instance;

    private bool _serverActive;
    private SteamId _id;
    private RepoNetworkSocketManager? _socketManager;
    
    internal bool ServerActive => _instance is not null && _serverActive;
    
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

    public void StartSocketServer(SteamId id)
    {
        _id = id;
        _socketManager = SteamNetworkingSockets.CreateRelaySocket<RepoNetworkSocketManager>();
        _serverActive = true;
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
            // connection.Flush();
            
            // Logging.Info($"[SendMessageToClients] Client: {connection.Id} Result: {result}");
        }
    }
}