using System.Collections.Generic;
using System.Linq;
using RepoSteamNetworking.Networking;
using RepoSteamNetworking.Utils;
using Steamworks;

namespace RepoSteamNetworking.API;

public static class NetworkExtensions
{
    public static SteamId GetSteamId(this PlayerAvatar playerAvatar)
    {
        if (!ulong.TryParse(playerAvatar.steamID, out var avatarSteamId))
        {
            Logging.Error("PlayerAvatar steam id is invalid!");
            return default;
        }

        return avatarSteamId;
    }

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

    public static void SendPacket<TPacket>(this SteamId steamId, TPacket packet)
        where TPacket : NetworkPacket
    {
        packet.Header.Target = steamId;
        RepoSteamNetwork.SendPacket(packet, NetworkDestination.PacketTarget);
    }

    public static void SendPacket<TPacket>(this IEnumerable<SteamId> steamIds, TPacket packet)
        where TPacket : NetworkPacket
    {
        foreach (var steamId in steamIds)
            steamId.SendPacket(packet);
    }

    public static void SendPacket<TPacket>(this PlayerAvatar playerAvatar, TPacket packet)
        where TPacket : NetworkPacket
    {
        var steamId = playerAvatar.GetSteamId();
        steamId.SendPacket(packet);
    }

    public static void SendPacket<TPacket>(this IEnumerable<PlayerAvatar> playerAvatars, TPacket packet)
        where TPacket : NetworkPacket
    {
        foreach (var playerAvatar in playerAvatars)
            playerAvatar.SendPacket(packet);
    }
}