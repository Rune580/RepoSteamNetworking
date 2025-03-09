using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using Steamworks;
using Steamworks.Data;

namespace RepoSteamNetworking.Patches;

public static class SteamManagerPatches
{
    [HarmonyPatch(typeof(SteamManager), nameof(SteamManager.OnLobbyCreated))]
    public static class OnLobbyCreatedPatch
    {
        // ReSharper disable once InconsistentNaming
        [SuppressMessage("Method Declaration", "Harmony003:Harmony non-ref patch parameters modified")]
        public static void Postfix(Lobby _lobby)
        {
            var id = _lobby.Owner.Id;
            
            RepoSteamNetworkingPlugin.SocketManager = SteamNetworkingSockets.CreateRelaySocket<RepoNetworkSocketManager>();
        }
    }

    [HarmonyPatch(typeof(SteamManager), nameof(SteamManager.OnLobbyEntered))]
    public static class OnLobbyEnteredPatch
    {
        // ReSharper disable once InconsistentNaming
        [SuppressMessage("Method Declaration", "Harmony003:Harmony non-ref patch parameters modified")]
        public static void Postfix(Lobby _lobby)
        {
            var id = _lobby.Owner.Id;

            RepoSteamNetworkingPlugin.ConnectionManager = SteamNetworkingSockets.ConnectRelay<RepoNetworkConnectionManager>(id);
            
            var result = RepoSteamNetworkingPlugin.ConnectionManager.Connection.SendMessage("Hello Mario"u8.ToArray());
            Logging.Info($"Send message (Lobby Entered): {result})");
            
            // RepoSteamNetworkingPlugin.ConnectionManager.Receive();
        }
    }
    
    [HarmonyPatch(typeof(SteamManager), nameof(SteamManager.OnLobbyMemberJoined))]
    public static class OnLobbyMemberJoinedPatch
    {
        // ReSharper disable once InconsistentNaming
        [SuppressMessage("Method Declaration", "Harmony003:Harmony non-ref patch parameters modified")]
        public static void Postfix()
        {
            var connnected = RepoSteamNetworkingPlugin.SocketManager.Connected;
            foreach (var connection in connnected)
            {
                connection.SendMessage("Hello Mario"u8.ToArray());
                connection.Flush();
            }
        }
    }
}