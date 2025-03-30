using RepoSteamNetworking.API;
using RepoSteamNetworking.Networking.Serialization;

namespace RepoSteamNetworking.Networking.Packets;

internal class HandshakeStartAuthPacket : NetworkPacket<HandshakeStartAuthPacket>
{
    /// <summary>
    /// Key assigned to current connection to be used to fetch the auth key from the lobby.
    /// </summary>
    public string ClientKey { get; set; } = string.Empty;
    protected override void WriteData(SocketMessage socketMessage)
    {
        socketMessage.Write(ClientKey);
    }

    protected override void ReadData(SocketMessage socketMessage)
    {
        ClientKey = socketMessage.Read<string>();
    }
}