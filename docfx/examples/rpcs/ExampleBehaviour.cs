public partial class ExampleBehaviour : MonoBehaviour
{
    
}
using UnityEngine;

public partial class ExampleBehaviour : MonoBehaviour
{
    public void ExampleRPC(string message, Vector3 position)
    {
        Debug.Log($"We got a message!\n\t{message} at {position}");
    }
}
using UnityEngine;
using RepoSteamNetworking.API.Unity;

public partial class ExampleBehaviour : MonoBehaviour
{
    [RepoSteamRPC]
    public void ExampleRPC(string message, Vector3 position)
    {
        Debug.Log($"We got a message!\n\t{message} at {position}");
    }
}
using UnityEngine;
using RepoSteamNetworking.API;
using RepoSteamNetworking.API.Unity;

public partial class ExampleBehaviour : MonoBehaviour
{
    [RepoSteamRPC(RPCTarget.Clients)]
    public void ExampleRPC(string message, Vector3 position)
    {
        Debug.Log($"We got a message!\n\t{message} at {position}");
    }
}
using UnityEngine;
using RepoSteamNetworking.API;
using RepoSteamNetworking.API.Unity;

public partial class ExampleBehaviour : MonoBehaviour
{
    [RepoSteamRPC(RPCTarget.Clients)]
    public void ExampleRPC(string message, Vector3 position)
    {
        Debug.Log($"We got a message!\n\t{message} at {position}");
    }

    public void Start()
    {
        Example("Hello we have Started!", transform.position);
    }
}