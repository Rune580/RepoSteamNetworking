using RepoSteamNetworking.API;
using RepoSteamNetworking.API.Asset;
using RepoSteamNetworking.API.Unity;
using RepoSteamNetworking.Networking.Data;
using RepoSteamNetworking.Utils;
using UnityEngine;

namespace RepoSteamNetworking.Networking.Packets;

internal class InstantiateNetworkedPrefabServerPacket : NetworkPacket<InstantiateNetworkedPrefabServerPacket>
{
    public AssetReference Prefab { get; set; }
    public bool HasTarget { get; private set; }
    public NetworkTransform TargetTransform { get; private set; }
    public Vector3 Position { get; set; }
    public Quaternion Rotation { get; set; }

    public void SetTargetTransform(Transform? target)
    {
        HasTarget = false;

        if (target is null)
            return;

        var networkIdentity = target.GetComponentInParent<RepoSteamNetworkIdentity>();
        if (networkIdentity is null)
            return;

        var path = target.GetPathFromParent(networkIdentity.transform);

        HasTarget = true;
        TargetTransform = new NetworkTransform
        {
            networkId = networkIdentity.NetworkId,
            path = path
        };
    }
    
    protected override void WriteData(SocketMessage socketMessage)
    {
        socketMessage.Write(Prefab);
        socketMessage.Write(HasTarget);
        if (HasTarget)
            socketMessage.Write(TargetTransform);
        socketMessage.Write(Position);
        socketMessage.Write(Rotation);
    }

    protected override void ReadData(SocketMessage socketMessage)
    {
        Prefab = socketMessage.Read<AssetReference>();
        HasTarget = socketMessage.Read<bool>();
        if (HasTarget)
            TargetTransform = socketMessage.Read<NetworkTransform>();
        Position = socketMessage.Read<Vector3>();
        Rotation = socketMessage.Read<Quaternion>();
    }
}