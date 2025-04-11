using RepoSteamNetworking.API;
using RepoSteamNetworking.API.Asset;
using RepoSteamNetworking.Networking.Data;
using RepoSteamNetworking.Networking.Serialization;
using UnityEngine;

namespace RepoSteamNetworking.Networking.Packets;

internal class InstantiateNetworkedPrefabClientPacket : NetworkPacket<InstantiateNetworkedPrefabClientPacket>
{
    public uint NetworkId { get; set; }
    public PrefabReference Prefab { get; set; }
    public bool HasTarget { get; private set; }
    public SerializableNetworkTransform TargetTransform { get; private set; }
    public Vector3 Position { get; set; }
    public Quaternion Rotation { get; set; }

    public void WithServerData(InstantiateNetworkedPrefabServerPacket serverData)
    {
        Prefab = serverData.Prefab;
        HasTarget = serverData.HasTarget;
        TargetTransform = serverData.TargetTransform;
        Position = serverData.Position;
        Rotation = serverData.Rotation;
    }
    
    protected override void WriteData(SocketMessage socketMessage)
    {
        socketMessage.Write(NetworkId);
        socketMessage.Write(Prefab);
        socketMessage.Write(HasTarget);
        if (HasTarget)
            socketMessage.Write(TargetTransform);
        socketMessage.Write(Position);
        socketMessage.Write(Rotation);
    }

    protected override void ReadData(SocketMessage socketMessage)
    {
        NetworkId = socketMessage.Read<uint>();
        Prefab = socketMessage.Read<PrefabReference>();
        HasTarget = socketMessage.Read<bool>();
        if (HasTarget)
            TargetTransform = socketMessage.Read<SerializableNetworkTransform>();
        Position = socketMessage.Read<Vector3>();
        Rotation = socketMessage.Read<Quaternion>();
    }
}