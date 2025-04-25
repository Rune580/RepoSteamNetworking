# Network Transforms

> [!WARNING]
> The current implementation of NetworkTransforms is fairly basic and isn't well optimized.
> It functions fairly well, but there could potentially be issues.

[NetworkTransform](../../api/RepoSteamNetworking.Networking.Unity.NetworkTransform.yml) is a component that networks a GameObject's position, rotation, and scale
over the network.

Add this component to any Transforms of a GameObject that you want networked.

## Interpolation
Interpolation is supported by setting the bool [doInterpolation](../../api/RepoSteamNetworking.Networking.Unity.NetworkTransform.yml#RepoSteamNetworking_Networking_Unity_NetworkTransform_doInterpolation) to true.
This will cause the GameObject to interpolate between its current transform state and the next transform state.