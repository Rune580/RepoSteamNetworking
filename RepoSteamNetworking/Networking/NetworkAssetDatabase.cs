using System.Collections.Generic;
using System.Linq;
using RepoSteamNetworking.API.Asset;
using RepoSteamNetworking.Utils;
using UnityEngine;

namespace RepoSteamNetworking.Networking;

internal static class NetworkAssetDatabase
{
    private static readonly Dictionary<AssetBundleReference, NetworkAssetBundle> AssetBundles = new();
    
    public static AssetBundleReference RegisterAssetBundle(AssetBundle assetBundle, string modGuid, string bundleName, bool managed)
    {
        AssetBundleReference bundleRef = (modGuid, bundleName);
        
        AssetBundles[bundleRef] = new NetworkAssetBundle
        {
            Bundle = assetBundle,
            Managed = managed
        };
        
        Logging.Info($"Registered Networked AssetBundle {bundleRef.ToString()}!");
        
        return bundleRef;
    }

    public static T? LoadAsset<T>(PrefabReference prefabRef)
        where T : Object
    {
        if (!AssetBundles.TryGetValue(prefabRef.BundleReference, out var networkBundle))
        {
            Logging.Error($"Failed to find AssetBundle with reference: {prefabRef.BundleReference}");
            return null;
        }

        if (networkBundle.Managed) // TODO
            return null;

        return networkBundle.Bundle.LoadAsset<T>(prefabRef.assetPath);
    }

    public static AssetBundleRequest? LoadAssetAsync<T>(PrefabReference prefabRef)
        where T : Object
    {
        if (!AssetBundles.TryGetValue(prefabRef.BundleReference, out var networkBundle))
        {
            Logging.Error($"Failed to find AssetBundle with reference: {prefabRef.BundleReference}");
            return null;
        }

        if (networkBundle.Managed) // TODO
            return null;

        return networkBundle.Bundle.LoadAssetAsync<T>(prefabRef.assetPath);
    }

    public static IEnumerable<PrefabReference> GetAllAssets(AssetBundleReference bundleRef)
    {
        var networkBundle = AssetBundles[bundleRef];
        var bundle = networkBundle.Bundle;

        var assetPaths = bundle.GetAllAssetNames();

        return assetPaths.Select(path => bundleRef.GetAssetReference(path));
    }

    private struct NetworkAssetBundle
    {
        public AssetBundle Bundle { get; set; }
        public bool Managed { get; set; }
    }
}