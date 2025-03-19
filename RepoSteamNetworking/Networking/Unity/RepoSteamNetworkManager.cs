using System.Collections.Generic;
using RepoSteamNetworking.API.Asset;
using RepoSteamNetworking.API.Unity;
using RepoSteamNetworking.Networking.Data;
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

    internal void InstantiateNetworkPrefab(uint networkId, AssetReference assetRef, Vector3 position, Quaternion rotation)
    {
        var prefab = NetworkAssetDatabase.LoadAsset<GameObject>(assetRef);
        if (prefab is null)
        {
            Logging.Error($"Failed to instantiate network prefab {assetRef.ToString()}! Verify the asset path and make sure you registered the AssetBundle!");
            return;
        }
        var wasPrefabActive = prefab.activeSelf;
        prefab.SetActive(false);
        
        var instance = Instantiate(prefab, position, rotation);
        var networkIdentity = instance.AddComponent<RepoSteamNetworkIdentity>();
        
        networkIdentity.SetNetworkId(networkId);
        instance.SetActive(wasPrefabActive);
    }

    internal void InstantiateNetworkPrefab(uint networkId, AssetReference assetRef, NetworkTransform targetTransform, Vector3 position, Quaternion rotation)
    {
        var prefab = NetworkAssetDatabase.LoadAsset<GameObject>(assetRef);
        if (prefab is null)
        {
            Logging.Error($"Failed to instantiate network prefab {assetRef.ToString()}! Verify the asset path and make sure you registered the AssetBundle!");
            return;
        }
        var wasPrefabActive = prefab.activeSelf;
        prefab.SetActive(false);

        if (!_networkObjects.TryGetValue(targetTransform.networkId, out var targetIdentity))
        {
            Logging.Error($"Failed to find target transform with network id: {targetTransform.networkId}!");
            return;
        }

        var target = targetIdentity.transform.Find(targetTransform.path);
        if (target is null)
        {
            Logging.Error($"Failed to find target transform with path: {targetTransform.path}!");
            return;
        }

        var instance = Instantiate(prefab, position, rotation, target);
        var networkIdentity = instance.AddComponent<RepoSteamNetworkIdentity>();
        
        networkIdentity.SetNetworkId(networkId);
        instance.SetActive(wasPrefabActive);
    }
    
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