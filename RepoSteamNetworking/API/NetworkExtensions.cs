using System.Collections.Generic;
using System.Linq;
using RepoSteamNetworking.Networking;
using RepoSteamNetworking.Utils;
using Steamworks;

namespace RepoSteamNetworking.API;

/// <summary>
/// Provides extension methods for retrieving Steam IDs, PlayerAvatars, and sending network packets to players or groups of players.
/// </summary>
public static class NetworkExtensions
{
    /// <summary>
    /// Retrieves the Steam ID associated with the specified PlayerAvatar instance.
    /// </summary>
    /// <param name="playerAvatar">The PlayerAvatar instance from which to extract the Steam ID.</param>
    /// <returns>
    /// A SteamId representing the player's Steam ID.
    /// Returns a SteamId with a value of <c>0</c> if the ID or the PlayerAvatar instance is invalid or cannot be parsed.
    /// Validate the SteamId by calling <see cref="SteamId.IsValid"/> on the returned value.
    /// </returns>
    public static SteamId GetSteamId(this PlayerAvatar playerAvatar)
    {
        if (!ulong.TryParse(playerAvatar.steamID, out var avatarSteamId))
        {
            Logging.Error("PlayerAvatar steam id is invalid!");
            return default;
        }

        return avatarSteamId;
    }

    /// <summary>
    /// Retrieves the PlayerAvatar instance associated with the specified Steam ID.
    /// </summary>
    /// <param name="steamId">The SteamId for which to find the associated PlayerAvatar instance.</param>
    /// <returns>
    /// A PlayerAvatar instance corresponding to the specified Steam ID.
    /// Returns <c>null</c> if no matching PlayerAvatar is found or if the Steam ID is invalid.
    /// </returns>
    public static PlayerAvatar? GetPlayerAvatar(this SteamId steamId)
    {
        var playerAvatar = GameDirector.instance.PlayerList.FirstOrDefault(avatar =>
        {
            if (!ulong.TryParse(avatar.steamID, out var avatarSteamId))
                return false;
            
            return avatarSteamId == steamId.Value;
        });
        
        return playerAvatar;
    }

    /// <summary>
    /// Sends a network packet to a specific Steam ID.
    /// </summary>
    /// <typeparam name="TPacket">The type of the network packet to be sent. Must inherit from the NetworkPacket class.</typeparam>
    /// <param name="steamId">The Steam ID of the target to which the packet will be sent.</param>
    /// <param name="packet">The network packet to be sent to the specified Steam ID.</param>
    public static void SendPacket<TPacket>(this SteamId steamId, TPacket packet)
        where TPacket : NetworkPacket
    {
        packet.Header.Target = steamId;
        RepoSteamNetwork.SendPacket(packet, NetworkDestination.PacketTarget);
    }

    /// <summary>
    /// Sends a network packet to a collection of Steam IDs.
    /// </summary>
    /// <typeparam name="TPacket">The type of the network packet to be sent. Must inherit from the NetworkPacket class.</typeparam>
    /// <param name="steamIds">The collection of Steam IDs representing the targets to which the packet will be sent.</param>
    /// <param name="packet">The network packet to be sent to the specified Steam IDs.</param>
    public static void SendPacket<TPacket>(this IEnumerable<SteamId> steamIds, TPacket packet)
        where TPacket : NetworkPacket
    {
        foreach (var steamId in steamIds)
            steamId.SendPacket(packet);
    }

    /// <summary>
    /// Sends a network packet to the specified PlayerAvatar by using their associated Steam ID.
    /// </summary>
    /// <typeparam name="TPacket">The type of the network packet to send, which must inherit from NetworkPacket.</typeparam>
    /// <param name="playerAvatar">The PlayerAvatar instance representing the target player to whom the packet will be sent.</param>
    /// <param name="packet">The network packet to be sent to the specified PlayerAvatar.</param>
    public static void SendPacket<TPacket>(this PlayerAvatar playerAvatar, TPacket packet)
        where TPacket : NetworkPacket
    {
        var steamId = playerAvatar.GetSteamId();
        steamId.SendPacket(packet);
    }

    /// <summary>
    /// Sends a network packet to a collection of PlayerAvatar instances.
    /// </summary>
    /// <typeparam name="TPacket">The type of the network packet to send, which must inherit from NetworkPacket.</typeparam>
    /// <param name="playerAvatars">The collection of PlayerAvatar instances representing the target players to whom the packet will be sent.</param>
    /// <param name="packet">The network packet to be sent to the specified players.</param>
    public static void SendPacket<TPacket>(this IEnumerable<PlayerAvatar> playerAvatars, TPacket packet)
        where TPacket : NetworkPacket
    {
        foreach (var playerAvatar in playerAvatars)
            playerAvatar.SendPacket(packet);
    }
}