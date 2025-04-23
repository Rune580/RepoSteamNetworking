using RepoSteamNetworking.API;
using RepoSteamNetworking.Utils;
using UnityEngine;

namespace RepoSteamNetworking.Networking.Unity;

[RequireComponent(typeof(Rigidbody), typeof(NetworkTransform))]
[DisallowMultipleComponent]
public class NetworkRigidbody : MonoBehaviour
{
    private Rigidbody _rigidbody = null!;
    
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();

        if (!_rigidbody)
        {
            Logging.Warn("No Rigidbody attached!");
            return;
        }
        
        SetupRigidbody();
    }

    private void SetupRigidbody()
    {
        if (!RepoSteamNetwork.IsServer)
        {
            _rigidbody.isKinematic = true;
        }
    }
}