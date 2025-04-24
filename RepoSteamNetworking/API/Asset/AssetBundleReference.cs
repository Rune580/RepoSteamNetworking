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

    /// <summary>
    /// Retrieves a specific prefab reference associated with the given asset path within the asset bundle.
    /// </summary>
    /// <param name="assetPath">The path of the asset within the bundle to create a prefab reference for.</param>
    /// <returns>A <see cref="RepoSteamNetworking.API.Asset.PrefabReference"/> corresponding to the specified asset path.</returns>
    public PrefabReference GetPrefabReference(string assetPath) => new(this, assetPath);

    /// <summary>
    /// Retrieves all assets within the specified asset bundle reference.
    /// </summary>
    /// <returns>An enumerable collection of <see cref="RepoSteamNetworking.API.Asset.PrefabReference"/> representing all assets in the bundle.</returns>
    public IEnumerable<PrefabReference> GetAllAssets() => NetworkAssetDatabase.GetAllAssets(this);

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