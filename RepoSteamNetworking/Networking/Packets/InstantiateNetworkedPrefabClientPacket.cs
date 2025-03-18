using RepoSteamNetworking.API;
using RepoSteamNetworking.API.Asset;
using RepoSteamNetworking.Networking.Data;
using UnityEngine;

namespace RepoSteamNetworking.Networking.Packets;

internal class InstantiateNetworkedPrefabClientPacket : NetworkPacket<InstantiateNetworkedPrefabClientPacket>
{
    public uint NetworkId { get; set; }
    public AssetReference Prefab { get; set; }
    public Vector3 Position { get; set; }
    public Quaternion Rotation { get; set; }
    
    protected override void WriteData(SocketMessage socketMessage)
    {
        socketMessage.Write(NetworkId);
        socketMessage.Write(Prefab);
        socketMessage.Write(Position);
        socketMessage.Write(Rotation);
    }

    protected override void ReadData(SocketMessage socketMessage)
    {
        NetworkId = socketMessage.Read<uint>();
        Prefab = socketMessage.Read<AssetReference>();
        Position = socketMessage.Read<Vector3>();
        Rotation = socketMessage.Read<Quaternion>();
    }
}