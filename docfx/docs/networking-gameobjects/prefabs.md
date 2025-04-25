# Prefabs

To instantiate prefabs over the network, you must first register the asset bundle the prefab belongs to.

## Registering AssetBundles

```csharp
using RepoSteamNetworking.API;
```
```csharp
var assetBundle = ...;

// Store this networkBundleReference somewhere for convenience
var networkBundleReference = RepoSteamNetwork.RegisterAssetBundle(assetBundle);

// example
MyPlugin.NetworkBundleRef = networkBundleReference;
```

## Instantiating Prefabs

```csharp
using RepoSteamNetworking.API;
```
```csharp
var networkBundleReference = MyPlugin.NetworkBundleRef; // See Registering AssetBundles

var prefabRef = networkBundleReference.GetPrefabReference("assets/Example Object.prefab");

RepoSteamNetwork.InstantiatePrefab(prefabRef);
```

See [RepoSteamNetwork](../../api/RepoSteamNetworking.API.RepoSteamNetwork.yml) for overloads of `InstanitatePrefab`.

### Modifying Prefabs Before Instantiation

Modifications are networked right before instantiation. The `WithModifications` action is run on the host.

```csharp
using RepoSteamNetworking.API;
```
```csharp
var networkBundleReference = MyPlugin.NetworkBundleRef; // See Registering AssetBundles

var prefabRef = networkBundleReference.GetPrefabReference("assets/Example Object.prefab");

var modifiedPrefabRef = prefabRef.WithModifications(prefab => 
    {
        var networkTransform = prefab.AddComponent<NetworkTransform>();
        networkTransform.doInterpolation = true;
        
        prefab.AddComponent<NetworkRigidbody>();
    });

RepoSteamNetwork.InstantiatePrefab(modifiedPrefabRef);
```