using System;

namespace RepoSteamNetworking.Networking.Data;

[Serializable]
internal struct NetworkTransform
{
    public uint networkId;
    public string path;
}