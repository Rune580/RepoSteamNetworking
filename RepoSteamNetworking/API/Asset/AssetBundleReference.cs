using System;
using System.Collections.Generic;
using RepoSteamNetworking.Networking;

namespace RepoSteamNetworking.API.Asset;

[Serializable]
public record struct AssetBundleReference
{
    public string modNamespace;
    public string bundleName;

    public override string ToString() => $"{modNamespace}:{bundleName}";

    public AssetReference GetAssetReference(string assetPath) => new(this, assetPath);

    public IEnumerable<AssetReference> GetAllAssets() => NetworkAssetDatabase.GetAllAssets(this);

    public static implicit operator AssetBundleReference((string, string) reference)
    {
        return new AssetBundleReference
        {
            modNamespace = reference.Item1,
            bundleName = reference.Item2
        };
    }

    public static implicit operator AssetBundleReference(string reference)
    {
        var parts = reference.Split(':', StringSplitOptions.RemoveEmptyEntries);

        return new AssetBundleReference
        {
            modNamespace = parts[0],
            bundleName = parts[1]
        };
    }
}