namespace RepoSteamNetworking.API;

public enum NetworkPermission : byte
{
    Host,
    Everyone,
    /// <summary>
    /// Only the owner of the <see cref="RepoSteamNetworking.API.Unity.RepoSteamNetworkIdentity"/> has permission.
    /// <remarks>Currently not implemented</remarks>
    /// </summary>
    Owner // TODO
}