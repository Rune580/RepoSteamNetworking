using RepoSteamNetworking.API.Unity;
using RepoSteamNetworking.Networking.NetworkedProperties;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace RepoSteamNetworking.Testing;

public partial class ExampleBehaviour : MonoBehaviour
{
    [NetworkedProperty(SendMethod = VariableChangeKind.Delta)]
    public float testFieldNumber;

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

        if (Keyboard.current.upArrowKey.isPressed)
        {
            TestFieldNumber += 2f * Time.deltaTime;
        }

        if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            TestPropertyNumber -= 1;
        }
        
        if (Keyboard.current.jKey.wasPressedThisFrame)
        {
            var networkIdentity = GetNetworkIdentity();
            Debug.Log($"Example Object: NetId: {networkIdentity.NetworkId}, SubId: {SubId}, TestFieldNumber: {TestFieldNumber}, TestPropertyNumber: {TestPropertyNumber}");
        }
    }
}