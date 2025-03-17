using RepoSteamNetworking.API.Unity;
using UnityEngine;

namespace RepoSteamNetworking.Testing;

public partial class ExampleBehaviour : MonoBehaviour
{
    [RepoSteamRPC]
    public void SendDataRPC(Vector3 position)
    {
        
    }

    private void Awake()
    {
        // GetNetworkIdentity().
    }
}