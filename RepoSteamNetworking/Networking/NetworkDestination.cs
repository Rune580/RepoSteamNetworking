namespace RepoSteamNetworking.Networking;

public enum NetworkDestination
{
    /// <summary>
    /// Only the host will receive the packet.
    /// </summary>
    HostOnly = 0,
    /// <summary>
    /// Only clients will receive the packet, if sending from a client, the packet will pass through the host without it being processed by the host.
    /// </summary>
    ClientsOnly = 1,
    /// <summary>
    /// All Clients, including the Host, will receive the packet.
    /// </summary>
    Everyone = 2,
    /// <summary>
    /// Send to Target SteamId defined in the <see cref="RepoSteamNetworking.API.NetworkPacket"/> <see cref="RepoSteamNetworking.Networking.Packets.PacketHeader"/>.
    /// If no target specified or the target is invalid, this packet will be dropped.
    /// </summary>
    PacketTarget = 3,
    /// <summary>
    /// Same as <see cref="Everyone"/> but the Sender doesn't receive the packet.
    /// </summary>
    EveryoneExcludingSender = 4,
}