using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using RepoSteamNetworking.Networking;
using Steamworks;
using Steamworks.Data;

namespace RepoSteamNetworking.Patches;

public static class SteamManagerPatches
{
    [HarmonyPatch(typeof(SteamManager), nameof(SteamManager.Awake))]
    public static class OnAwakePatch
    {
        // ReSharper disable once InconsistentNaming
        public static void Postfix(SteamManager __instance)
        {
            RepoNetworkingServer.CreateSingleton(__instance.gameObject);
            RepoNetworkingClient.CreateSingleton(__instance.gameObject);
        }
    }
    
    [HarmonyPatch(typeof(SteamManager), nameof(SteamManager.OnLobbyCreated))]
    public static class OnLobbyCreatedPatch
    {
        // ReSharper disable once InconsistentNaming
        [SuppressMessage("Method Declaration", "Harmony003:Harmony non-ref patch parameters modified")]
        public static void Postfix(Lobby _lobby)
        {
            var id = _lobby.Owner.Id;
            
            RepoNetworkingServer.Instance.StartSocketServer(id);
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
            
            RepoNetworkingClient.Instance.ConnectToServer(id);
        }
    }
    
    [HarmonyPatch(typeof(SteamManager), nameof(SteamManager.LeaveLobby))]
    public static class OnLeaveLobbyPatch
    {
        // ReSharper disable once InconsistentNaming
        [SuppressMessage("Method Declaration", "Harmony003:Harmony non-ref patch parameters modified")]
        public static void Prefix(SteamManager __instance)
        {
            if (__instance.currentLobby.IsOwnedBy(SteamClient.SteamId))
            {
                RepoNetworkingServer.Instance.StopSocketServer();
                RepoNetworkingClient.Instance.DisconnectFromServer();
            }
            else
            {
                RepoNetworkingClient.Instance.DisconnectFromServer();
            }
        }
    }
}