# Networked Physics

## Rigidbodies
Rigidbodies can be networked by adding a [NetworkTransform](../../api/RepoSteamNetworking.Networking.Unity.NetworkTransform.yml) and a [NetworkRigidbody](../../api/RepoSteamNetworking.Networking.Unity.NetworkRigidbody.yml) component to a GameObject that already has a Rigidbody component.

Rigidbodies will be run on the host, while client Rigidbodies will be kinematic. For better visuals on the client side, enable interpolation on the NetworkTransform.