using System.Collections.Generic;
using RepoSteamNetworking.API;
using RepoSteamNetworking.API.Asset;
using RepoSteamNetworking.API.Unity;
using RepoSteamNetworking.Networking.Data;
using RepoSteamNetworking.Networking.NetworkedProperties;
using RepoSteamNetworking.Networking.Registries;
using RepoSteamNetworking.Testing;
using RepoSteamNetworking.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RepoSteamNetworking.Networking.Unity;

public class RepoSteamNetworkManager : MonoBehaviour
{
    // TODO: Look into making this be something dynamic/configurable.
    private const float NetworkPropertySyncInterval = 1f / 30f;
    
    private static RepoSteamNetworkManager? _instance;
    public static RepoSteamNetworkManager Instance => _instance;
    
    private readonly Dictionary<uint, RepoSteamNetworkIdentity> _networkObjects = new();
    private ModNetworkGuidPalette _guidPalette = new();
    private BehaviourIdPalette _behaviourIdPalette = new();

    private float _networkPropertySyncTimer;
        
    internal uint NewNetworkId => field++;
    
    private void Awake()
    {
        _instance = this;
        
        hideFlags = HideFlags.HideAndDontSave;
        DontDestroyOnLoad(this);
    }

    private void Update()
    {
        _networkPropertySyncTimer += Time.deltaTime;
        while (_networkPropertySyncTimer >= NetworkPropertySyncInterval)
        {
            _networkPropertySyncTimer -= NetworkPropertySyncInterval;
            NetworkedPropertyManager.SyncNetworkedProperties();
        }
        
        if (Keyboard.current.hKey.wasPressedThisFrame)
        {
            if (!RepoNetworkingServer.Instance.ServerActive)
                return;

            var player = PlayerAvatar.instance;
            var pos = player.transform.position;

            var prefabRef = RepoSteamNetworkingPlugin.TestBundle.GetAssetReference("assets/Example Object.prefab");

            prefabRef = prefabRef.WithModifications(prefab =>
            {
                prefab.AddComponent<NetworkTransform>();
                prefab.AddComponent<NetworkRigidbody>();
                prefab.AddComponent<ExampleBehaviour>();
            });

            RepoSteamNetwork.InstantiatePrefab(prefabRef, pos);
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
    
    internal void SetModGuidPalette(ModNetworkGuidPalette palette)
    {
        _guidPalette = palette;

#if DEBUG
        foreach (var (paletteId, modGuid) in _guidPalette)
        {
            Logging.Info($"[ModNetworkGuidPalette] {modGuid} is registered as {paletteId}");
        }
#endif
    }

    internal void SetBehaviourIdPalette(BehaviourIdPalette palette)
    {
        _behaviourIdPalette = palette;
        
#if DEBUG
        foreach (var (paletteId, behaviour) in _behaviourIdPalette)
        {
            Logging.Info($"[BehaviourIdPalette] {behaviour} is registered as {paletteId}");
        }
#endif
    }

    internal string GetModGuid(uint paletteId) => _guidPalette.GetModGuid(paletteId);
    
    internal uint GetGuidPaletteId(string modGuid) => _guidPalette.GetPaletteId(modGuid);

    public string GetBehaviourClassName(uint behaviourId) => _behaviourIdPalette.GetClassName(behaviourId);
    
    public uint GetBehaviourId(string fullyQualifiedClassName) => _behaviourIdPalette.GetBehaviourId(fullyQualifiedClassName);
    
    internal RepoSteamNetworkIdentity GetNetworkIdentity(uint networkId) => _networkObjects[networkId];

    internal void InstantiateNetworkPrefab(uint networkId, PrefabReference prefabRef, Vector3 position, Quaternion rotation)
    {
        var prefab = NetworkAssetDatabase.LoadAsset<GameObject>(prefabRef);
        if (prefab is null)
        {
            Logging.Error($"Failed to instantiate network prefab {prefabRef.ToString()}! Verify the asset path and make sure you registered the AssetBundle!");
            return;
        }
        var wasPrefabActive = prefab.activeSelf;
        prefab.SetActive(false);
        
        var instance = Instantiate(prefab, position, rotation);
        if (prefabRef.HasModifications)
            prefabRef.ApplyModifications(instance);
        
        var networkIdentity = instance.AddComponent<RepoSteamNetworkIdentity>();
        
        networkIdentity.SetNetworkId(networkId);
        instance.SetActive(wasPrefabActive);
        prefab.SetActive(wasPrefabActive);
    }

    internal void InstantiateNetworkPrefab(uint networkId, PrefabReference prefabRef, SerializableNetworkTransform targetTransform, Vector3 position, Quaternion rotation)
    {
        var prefab = NetworkAssetDatabase.LoadAsset<GameObject>(prefabRef);
        if (prefab is null)
        {
            Logging.Error($"Failed to instantiate network prefab {prefabRef.ToString()}! Verify the asset path and make sure you registered the AssetBundle!");
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
        if (prefabRef.HasModifications)
            prefabRef.ApplyModifications(instance);
        
        var networkIdentity = instance.AddComponent<RepoSteamNetworkIdentity>();
        
        networkIdentity.SetNetworkId(networkId);
        instance.SetActive(wasPrefabActive);
        prefab.SetActive(wasPrefabActive);
    }
    
    internal static void CreateSingleton(GameObject parent)
    {
        if (_instance is not null)
        {
            Logging.Warn($"{nameof(RepoSteamNetworkManager)} already initialized!");
            return;
        }
        
        VersionCompatRegistry.InitRegistry();
        ModNetworkGuidRegistry.Init();
        BehaviourIdRegistry.Init();
        
        Instantiate(new GameObject("RepoSteamNetworkManager"), parent.transform)
            .AddComponent<RepoSteamNetworkManager>();
        
        Logging.Info("Created RepoSteamNetworkManager");
    }
}