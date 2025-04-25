using System.Collections.Generic;
using System.Linq;
using RepoSteamNetworking.Networking.Unity;
using RepoSteamNetworking.Utils;
using UnityEngine;

namespace RepoSteamNetworking.API.Unity;

[DisallowMultipleComponent]
public class RepoSteamNetworkIdentity : MonoBehaviour
{
    /// <summary>
    /// Represents the unique identifier for a networked object.
    /// This identifier is assigned to an object when it is registered in the network
    /// and remains constant for the lifetime of the object.
    /// </summary>
    public uint NetworkId { get; private set; }
    public bool IsValid { get; private set; }

    private readonly Dictionary<string, ModScopedSubIdentityMap> _modSubIdentities = new();
    
    private void Awake()
    {
        if (GetComponentsInChildren<RepoSteamNetworkIdentity>().Length > 1)
            Logging.Warn($"There should only be one {nameof(RepoSteamNetworkIdentity)} attached to a prefab! {gameObject.name} Has more than 1!");

        foreach (var subIdentity in GetComponentsInChildren<Component>().OfType<ISteamNetworkSubIdentity>())
            RegisterSubIdentity(subIdentity);
    }

    private void OnEnable()
    {
        if (!IsValid)
        {
            Logging.Warn($"NetworkIdentity of {gameObject.name} is invalid!");
            return;
        }
        
        RepoSteamNetworkManager.Instance.RegisterNetworkIdentity(this);
    }

    private void OnDestroy()
    {
        RepoSteamNetworkManager.Instance.UnRegisterNetworkIdentity(this);
    }

    internal void SetNetworkId(uint networkId)
    {
        NetworkId = networkId;
        IsValid = true;
    }

    public void RegisterSubIdentity<T>(T target)
        where T : ISteamNetworkSubIdentity
    {
        if (!_modSubIdentities.TryGetValue(target.ModGuid, out var scopedSubIdentityMap))
            _modSubIdentities[target.ModGuid] = scopedSubIdentityMap = new ModScopedSubIdentityMap();
        
        scopedSubIdentityMap.RegisterSubIdentity(target);
    }

    public ISteamNetworkSubIdentity GetSubIdentity(string modGuid, uint subId)
    {
        return _modSubIdentities[modGuid][subId];
    }

    private class ModScopedSubIdentityMap
    {
        private readonly Dictionary<uint, ISteamNetworkSubIdentity> _networkedBehaviours = new();
        private uint NextId => field++;

        public void RegisterSubIdentity<T>(T target)
            where T : ISteamNetworkSubIdentity
        {
            // Don't register twice
            if (target.IsValid && _networkedBehaviours.ContainsKey(target.SubId))
                return;
            
            target.SubId = NextId;
            target.IsValid = true;
        
            _networkedBehaviours[target.SubId] = target;
        }
        
        public ISteamNetworkSubIdentity this[uint subId] => _networkedBehaviours[subId];
    }
}