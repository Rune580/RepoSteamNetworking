using RepoSteamNetworking.API;
using RepoSteamNetworking.Networking.Data;

namespace RepoSteamNetworking.Networking.Packets;

public class HandshakeStatusPacket : NetworkPacket<HandshakeStatusPacket>
{
    public bool Success { get; set; }
    
    protected override void WriteData(SocketMessage socketMessage)
    {
        socketMessage.Write(Success);
    }

    protected override void ReadData(SocketMessage socketMessage)
    {
        Success = socketMessage.Read<bool>();
    }
}