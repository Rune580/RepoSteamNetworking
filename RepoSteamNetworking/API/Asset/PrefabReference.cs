using System;
using System.Collections.Generic;
using RepoSteamNetworking.Networking;
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

    public PrefabReference(PrefabReference other)
    {
        modNamespace = other.modNamespace;
        bundleName = other.bundleName;
        assetPath = other.assetPath;
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

    public PrefabReference WithModifications(Action<GameObject> modifyPrefabAction)
    {
        var newPrefabRef = new PrefabReference(this)
        {
            _modifyPrefabAction = modifyPrefabAction
        };
        return newPrefabRef;
    }

    internal void ApplyModifications(GameObject prefab)
    {
        if (_modifyPrefabAction is null)
            return;

        var components = prefab.GetComponentsInChildren<MonoBehaviour>();

        var componentFieldValues = new Dictionary<string, Dictionary<string, object?>>();

        foreach (var component in components)
        {
            component.GetAllSerializedFields();
        }
            
        
        _modifyPrefabAction.Invoke(prefab);
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