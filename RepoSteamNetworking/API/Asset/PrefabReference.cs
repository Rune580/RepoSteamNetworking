using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using RepoSteamNetworking.Networking;
using RepoSteamNetworking.Prefab;
using RepoSteamNetworking.Prefab.Modifications;
using UnityEngine;
using Object = UnityEngine.Object;

#if DEBUG
using RepoSteamNetworking.Utils;
#endif

namespace RepoSteamNetworking.API.Asset;

[Serializable]
public struct PrefabReference
{
    public string modNamespace;
    public string bundleName;
    public string assetPath;

    [NonSerialized]
    private List<Action<GameObject>> _modifyPrefabActions = [];
    public BasePrefabModification[] Modifications = [];
    
    public bool HasModifications => _modifyPrefabActions.Count > 0 || Modifications.Length > 0;

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

    /// <summary>
    /// Creates a new instance of <see cref="PrefabReference"/> with added modifications applied to the prefab GameObject.
    /// </summary>
    /// <param name="modifyPrefabAction">
    /// An action to be executed on a <see cref="GameObject"/> that defines how the GameObject
    /// will be modified.
    /// </param>
    /// <returns>
    /// A new instance of <see cref="PrefabReference"/> with the applied modifications.
    /// </returns>
    [Pure]
    public PrefabReference WithModifications(Action<GameObject> modifyPrefabAction)
    {
        var newPrefabRef = new PrefabReference(this)
        {
            _modifyPrefabActions = [.._modifyPrefabActions, modifyPrefabAction]
        };
        return newPrefabRef;
    }

    internal void CreateModifications(GameObject prefab)
    {
        if (_modifyPrefabActions.Count <= 0)
            return;

        var originalState = new PrefabState(prefab);

        foreach (var modifyPrefabAction in _modifyPrefabActions)
            modifyPrefabAction.Invoke(prefab);
        
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