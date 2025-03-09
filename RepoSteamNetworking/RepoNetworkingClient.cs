using System.Text;
using Steamworks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RepoSteamNetworking;

public class RepoNetworkingClient : MonoBehaviour
{
    private static RepoNetworkingClient? _instance;
    public static RepoNetworkingClient Instance => _instance;
    
    private bool _clientActive;
    private SteamId _hostId;
    private RepoNetworkConnectionManager? _connectionManager;
    
    internal static void CreateSingleton(GameObject parent)
    {
        if (_instance is not null)
        {
            Logging.Warn($"{nameof(RepoNetworkingClient)} already initialized!");
            return;
        }
        
        Instantiate(new GameObject("RepoNetworkingClient"), parent.transform)
            .AddComponent<RepoNetworkingClient>();
        
        Logging.Info("Created RepoNetworkingServer");
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
        
        if (Keyboard.current.hKey.wasPressedThisFrame)
        {
            SendMessageToServer("Hello Mario");
        }
    }

    public void ConnectToServer(SteamId hostId)
    {
        _hostId = hostId;
        _connectionManager = SteamNetworkingSockets.ConnectRelay<RepoNetworkConnectionManager>(_hostId);
        _clientActive = true;
    }

    public void DisconnectFromServer()
    {
        _connectionManager?.Close();
        _clientActive = false;
    }

    public void SendMessageToServer(string message)
    {
        if (_connectionManager is null)
        {
            Logging.Info("No connection to server, can't send message!");
            return;
        }
        
        var result = _connectionManager.Connection.SendMessage(Encoding.UTF8.GetBytes(message));
        _connectionManager.Connection.Flush();
        
        Logging.Info($"[SendMessageToServer] Result: {result}");
    }
}