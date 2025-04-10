using System;
using System.Diagnostics.Contracts;
using RepoSteamNetworking.Networking;
using RepoSteamNetworking.Prefab;
using RepoSteamNetworking.Prefab.Modifications;
using RepoSteamNetworking.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RepoSteamNetworking.API.Asset;

[Serializable]
public struct PrefabReference
{
    public string modNamespace;
    public string bundleName;
    public string assetPath;

    [NonSerialized]
    private Action<GameObject>? _modifyPrefabAction;
    public BasePrefabModification[] Modifications = [];
    
    public bool HasModifications => _modifyPrefabAction is not null || Modifications.Length > 0;

    public PrefabReference(PrefabReference other)
    {
        modNamespace = other.modNamespace;
        bundleName = other.bundleName;
        assetPath = other.assetPath;
        Modifications = other.Modifications;
    }

    public PrefabReference(AssetBundleReference bundleRef, string assetPath)
    {
        modNamespace = bundleRef.modNamespace;
        bundleName = bundleRef.bundleName;
        this.assetPath = assetPath;
    }

    public AssetBundleReference BundleReference => (modNamespace, bundleName);

    public T? LoadAsset<T>()
        where T : Object
    {
        return NetworkAssetDatabase.LoadAsset<T>(this);
    }

    public AssetBundleRequest? LoadAssetAsync<T>()
        where T : Object
    {
        return NetworkAssetDatabase.LoadAssetAsync<T>(this);
    }

    [Pure]
    public PrefabReference WithModifications(Action<GameObject> modifyPrefabAction)
    {
        var newPrefabRef = new PrefabReference(this)
        {
            _modifyPrefabAction = modifyPrefabAction
        };
        return newPrefabRef;
    }

    internal void CreateModifications(GameObject prefab)
    {
        if (_modifyPrefabAction is null)
            return;

        var originalState = new PrefabState(prefab);
        _modifyPrefabAction.Invoke(prefab);
        var modifiedState = new PrefabState(prefab);
        
        Modifications = originalState.GetModifications(modifiedState).AsArray();

#if DEBUG
        Logging.Info($"Found {Modifications.Length} number of Modifications on {prefab.name}!");
#endif
    }

    internal void ApplyModifications(GameObject prefab)
    {
        if (Modifications.Length == 0)
            return;
        
        var modifications = new PrefabModifications(Modifications);
        modifications.ApplyModifications(prefab);
    }

    public override string ToString() => $"{modNamespace}:{bundleName}:{assetPath}";
    
    public static implicit operator PrefabReference(string reference)
    {
        var parts = reference.Split(':', StringSplitOptions.RemoveEmptyEntries);

        return new PrefabReference
        {
            modNamespace = parts[0],
            bundleName = parts[1],
            assetPath = parts[2]
        };
    }
}