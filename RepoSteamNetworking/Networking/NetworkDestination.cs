namespace RepoSteamNetworking.Networking;

public enum NetworkDestination
{
    /// <summary>
    /// Only the host will receive the packet
    /// </summary>
    HostOnly = 0,
    /// <summary>
    /// All Clients, including the Host, will receive the packet
    /// </summary>
    Everyone = 1,
}