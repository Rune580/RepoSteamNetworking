using System.Collections.Generic;
using RepoSteamNetworking.API.Unity;
using RepoSteamNetworking.Utils;
using UnityEngine;

namespace RepoSteamNetworking.Networking.Unity;

public class RepoSteamNetworkManager : MonoBehaviour
{
    private static RepoSteamNetworkManager? _instance;
    public static RepoSteamNetworkManager Instance => _instance;

    private readonly Dictionary<uint, RepoSteamNetworkIdentity> _networkObjects = new();
    private uint NextId => field++;
    
    private void Awake()
    {
        _instance = this;
        
        hideFlags = HideFlags.HideAndDontSave;
        DontDestroyOnLoad(this);
    }

    public void RegisterNetworkIdentity(RepoSteamNetworkIdentity networkIdentity)
    {
        networkIdentity.SetNetworkId(NextId);
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
            .AddComponent<RepoNetworkingServer>();
        
        Logging.Info("Created RepoSteamNetworkManager");
    }
}