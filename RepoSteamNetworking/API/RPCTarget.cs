namespace RepoSteamNetworking.API;

/// <summary>
/// Specifies where the RPC will be executed.
/// </summary>
public enum RPCTarget
{
    /// <summary>
    /// RPC will only run on the host.
    /// </summary>
    Host,
    /// <summary>
    /// RPC will only run on clients.
    /// </summary>
    Clients,
    /// <summary>
    /// RPC will run on everyone.
    /// </summary>
    Everyone,
}