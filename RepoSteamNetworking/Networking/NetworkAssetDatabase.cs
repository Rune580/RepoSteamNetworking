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

    public static T? LoadAsset<T>(AssetReference assetRef)
        where T : Object
    {
        if (!AssetBundles.TryGetValue(assetRef.BundleReference, out var networkBundle))
        {
            Logging.Error($"Failed to find AssetBundle with reference: {assetRef.BundleReference}");
            return null;
        }

        if (networkBundle.Managed) // TODO
            return null;

        return networkBundle.Bundle.LoadAsset<T>(assetRef.assetPath);
    }

    public static AssetBundleRequest? LoadAssetAsync<T>(AssetReference assetRef)
        where T : Object
    {
        if (!AssetBundles.TryGetValue(assetRef.BundleReference, out var networkBundle))
        {
            Logging.Error($"Failed to find AssetBundle with reference: {assetRef.BundleReference}");
            return null;
        }

        if (networkBundle.Managed) // TODO
            return null;

        return networkBundle.Bundle.LoadAssetAsync<T>(assetRef.assetPath);
    }

    public static IEnumerable<AssetReference> GetAllAssets(AssetBundleReference bundleRef)
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