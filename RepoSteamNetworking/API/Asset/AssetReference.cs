using System;

namespace RepoSteamNetworking.API.Asset;

[Serializable]
public struct AssetReference
{
    public string modNamespace;
    public string bundleName;
    public string assetPath;

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