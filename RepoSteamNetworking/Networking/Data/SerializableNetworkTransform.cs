using System;

namespace RepoSteamNetworking.Networking.Data;

[Serializable]
internal struct SerializableNetworkTransform
{
    public uint networkId;
    public string path;
}