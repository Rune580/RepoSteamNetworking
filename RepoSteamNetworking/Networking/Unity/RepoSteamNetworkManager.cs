using System.Collections.Generic;
using RepoSteamNetworking.API.Unity;
using RepoSteamNetworking.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RepoSteamNetworking.Networking.Unity;

public class RepoSteamNetworkManager : MonoBehaviour
{
    private static RepoSteamNetworkManager? _instance;
    public static RepoSteamNetworkManager Instance => _instance;
    
    private readonly Dictionary<uint, RepoSteamNetworkIdentity> _networkObjects = new();
    
    internal uint NewNetworkId => field++;
    
    private void Awake()
    {
        _instance = this;
        
        hideFlags = HideFlags.HideAndDontSave;
        DontDestroyOnLoad(this);
    }

    private void Update()
    {
        if (Keyboard.current.hKey.wasPressedThisFrame)
        {
            if (!RepoNetworkingServer.Instance.ServerActive)
                return;

            // RepoSteamNetwork.InstantiatePrefab();
        }
    }

    public void RegisterNetworkIdentity(RepoSteamNetworkIdentity networkIdentity)
    {
        if (!networkIdentity.IsValid)
            return;
        
        _networkObjects[networkIdentity.NetworkId] = networkIdentity;
    }

    public void UnRegisterNetworkIdentity(RepoSteamNetworkIdentity networkIdentity)
    {
        if (!networkIdentity.IsValid)
            return;
        
        _networkObjects.Remove(networkIdentity.NetworkId);
    }
    
    internal RepoSteamNetworkIdentity GetNetworkIdentity(uint networkId) => _networkObjects[networkId];
    
    internal static void CreateSingleton(GameObject parent)
    {
        if (_instance is not null)
        {
            Logging.Warn($"{nameof(RepoSteamNetworkManager)} already initialized!");
            return;
        }
        
        VersionCompatRegistry.InitRegistry();
        
        Instantiate(new GameObject("RepoSteamNetworkManager"), parent.transform)
            .AddComponent<RepoSteamNetworkManager>();
        
        Logging.Info("Created RepoSteamNetworkManager");
    }
}