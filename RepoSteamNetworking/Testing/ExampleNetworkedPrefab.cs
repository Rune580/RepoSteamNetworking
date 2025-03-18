using UnityEngine;

namespace RepoSteamNetworking.Testing;

internal static class ExampleNetworkedPrefab
{
    public static GameObject GetPrefab()
    {
        var gameObject = new GameObject("ExampleNetworkedPrefab");
        gameObject.SetActive(false);
        
        gameObject.AddComponent<ExampleBehaviour>();
        
        return gameObject;
    }
}