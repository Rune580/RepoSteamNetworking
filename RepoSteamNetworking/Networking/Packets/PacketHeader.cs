using Steamworks;

namespace RepoSteamNetworking.Networking.Packets;

/// <summary>
/// Represents the header information for a network packet.
/// </summary>
public struct PacketHeader
{
    /// <summary>
    /// Represents a unique identifier for a network packet.
    /// This identifier is used to distinguish between different types of packets.
    /// <remarks>
    /// This will be set automatically when sending a <see cref="RepoSteamNetworking.API.NetworkPacket"/>. via <see cref="RepoSteamNetworking.API.RepoSteamNetwork.SendPacket"/>
    /// </remarks>
    /// </summary>
    public int PacketId;

    /// <summary>
    /// Specifies the network destination for a packet.
    /// <remarks>
    /// See <see cref="RepoSteamNetworking.Networking.NetworkDestination"/> for valid values.
    /// </remarks>
    /// </summary>
    public NetworkDestination Destination;

    /// <summary>
    /// Represents the Steam ID of the client that sent the packet.
    /// </summary>
    public SteamId Sender;

    /// <summary>
    /// Represents the target client of the packet.
    /// <remarks>
    /// May not be valid if the <see cref="RepoSteamNetworking.Networking.Packets.PacketHeader.Destination"/> is not set to <see cref="RepoSteamNetworking.Networking.NetworkDestination.PacketTarget"/>.
    /// Otherwise, the target must be valid.
    /// </remarks>
    /// </summary>
    public SteamId Target;
}