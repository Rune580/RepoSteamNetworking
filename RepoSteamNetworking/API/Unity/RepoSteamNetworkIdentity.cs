using System.Collections.Generic;
using System.Linq;
using RepoSteamNetworking.Networking.Unity;
using RepoSteamNetworking.Utils;
using UnityEngine;

namespace RepoSteamNetworking.API.Unity;

[DisallowMultipleComponent]
public class RepoSteamNetworkIdentity : MonoBehaviour
{
    public uint NetworkId { get; private set; }
    public bool IsValid { get; private set; }

    private readonly Dictionary<uint, ISteamNetworkSubIdentity> _networkedBehaviours = new();

    private uint NextId => field++;
    
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
        // Don't register twice
        if (target.IsValid && _networkedBehaviours.ContainsKey(target.SubId))
            return;
        
        target.SubId = NextId;
        target.IsValid = true;
        
        _networkedBehaviours[target.SubId] = target;
    }

    public ISteamNetworkSubIdentity GetSubIdentity(uint subId)
    {
        return _networkedBehaviours[subId];
    }
}