using System;

namespace RepoSteamNetworking.Networking.Serialization;

[Serializable]
public struct PrefabComponentData
{
    public string Path;
    public string ComponentTypeName;
    public int ComponentIndex;

    public string PathIndex => $"{Path}/{ComponentTypeName}.{ComponentIndex}";
}