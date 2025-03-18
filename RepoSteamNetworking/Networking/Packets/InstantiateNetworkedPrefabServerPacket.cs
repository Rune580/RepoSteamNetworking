using RepoSteamNetworking.API;
using RepoSteamNetworking.API.Asset;
using RepoSteamNetworking.Networking.Data;
using UnityEngine;

namespace RepoSteamNetworking.Networking.Packets;

internal class InstantiateNetworkedPrefabServerPacket : NetworkPacket<InstantiateNetworkedPrefabServerPacket>
{
    public AssetReference Prefab { get; set; }
    public Vector3 Position { get; set; }
    public Quaternion Rotation { get; set; }
    
    protected override void WriteData(SocketMessage socketMessage)
    {
        socketMessage.Write(Prefab);
        socketMessage.Write(Position);
        socketMessage.Write(Rotation);
    }

    protected override void ReadData(SocketMessage socketMessage)
    {
        Prefab = socketMessage.Read<AssetReference>();
        Position = socketMessage.Read<Vector3>();
        Rotation = socketMessage.Read<Quaternion>();
    }
}