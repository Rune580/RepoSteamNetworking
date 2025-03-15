using System.Collections.Generic;
using System.Linq;
using RepoSteamNetworking.API;
using Steamworks;

namespace RepoSteamNetworking.Utils;

internal static class SteamIdUtils
{
    private static readonly Dictionary<ulong, string> CachedSteamIds = new();
    
    public static string GetLobbyName(this SteamId steamId)
    {
        if (CachedSteamIds.TryGetValue(steamId, out var cachedName))
            return cachedName;
        
        var lobby = RepoSteamNetwork.GetCurrentLobby();
        SteamFriends.RequestUserInformation(steamId);

        // Use NameHistory so we don't accidentally dox people who use nicknames to refer to their friends real names.
        var names = lobby.Members.Where(friend => friend.Id == steamId)
            .SelectMany(friend => friend.NameHistory)
            .ToArray();

        if (names.Length <= 0)
            return $"SteamId: {steamId.Value}";
        
        var name = names.First();
        if (string.IsNullOrEmpty(name))
            return $"SteamId: {steamId.Value}";
        
        CachedSteamIds[steamId] = name;
        
        return name;
    }
}