using RepoSteamNetworking.API.Unity;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace RepoSteamNetworking.Testing;

public partial class ExampleBehaviour : MonoBehaviour
{
    [NetworkedProperty]
    public int testFieldNumber;

    [NetworkedProperty]
    public partial int TestPropertyNumber { get; set; }
    
    [RepoSteamRPC]
    public void DoExampleRPC(string message, Vector3 position)
    {
        Debug.Log($"Got message: {message}");
        Debug.Log($"Got position: {position}");
    }

    private void Update()
    {
        if (Keyboard.current.bKey.wasPressedThisFrame)
        {
            DoExample("Hello Mario", Random.insideUnitSphere);
        }
    }
}