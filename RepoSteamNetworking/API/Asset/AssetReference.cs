using System;
using RepoSteamNetworking.Networking;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RepoSteamNetworking.API.Asset;

[Serializable]
public struct AssetReference
{
    public string modNamespace;
    public string bundleName;
    public string assetPath;

    public AssetReference(AssetBundleReference bundleRef, string assetPath)
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

    public override string ToString() => $"{modNamespace}:{bundleName}:{assetPath}";
    
    public static implicit operator AssetReference(string reference)
    {
        var parts = reference.Split(':', StringSplitOptions.RemoveEmptyEntries);

        return new AssetReference
        {
            modNamespace = parts[0],
            bundleName = parts[1],
            assetPath = parts[2]
        };
    }
}