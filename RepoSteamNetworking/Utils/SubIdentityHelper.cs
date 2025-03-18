using System.Linq;
using RepoSteamNetworking.API.Unity;
using UnityEngine;

namespace RepoSteamNetworking.Utils;

internal static class SubIdentityHelper
{
    public static void RegisterAllSubIdentities(this GameObject gameObject)
    {
        var subIdentities = gameObject.GetComponentsInChildren<Component>()
            .OfType<ISteamNetworkSubIdentity>();

        RepoSteamNetworkIdentity? networkIdentity = null;

        foreach (var subIdentity in subIdentities)
        {
            networkIdentity ??= subIdentity.GetNetworkIdentity();
            networkIdentity.RegisterSubIdentity(subIdentity);
        }
    }
}