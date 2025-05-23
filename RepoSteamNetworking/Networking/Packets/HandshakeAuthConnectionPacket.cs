using RepoSteamNetworking.API;
using RepoSteamNetworking.Networking.Serialization;
using Steamworks;
using Steamworks.Data;

namespace RepoSteamNetworking.Networking.Packets;

internal class HandshakeAuthConnectionPacket : NetworkPacket<HandshakeAuthConnectionPacket>
{
    public SteamId LobbyId { get; private set; }
    public SteamId PlayerId { get; private set; }
    public string AuthKey { get; private set; } = string.Empty;

    internal void SetData(Lobby lobby, string clientKey)
    {
        LobbyId = lobby.Id;
        PlayerId = SteamClient.SteamId;
        AuthKey = lobby.GetMemberData(lobby.Owner, clientKey);
    }
    
    protected override void WriteData(SocketMessage socketMessage)
    {
        socketMessage.Write(LobbyId.Value);
        socketMessage.Write(PlayerId.Value);
        socketMessage.Write(AuthKey);
    }

    protected override void ReadData(SocketMessage socketMessage)
    {
        var lobbyId = socketMessage.Read<ulong>();
        var playerId = socketMessage.Read<ulong>();
        
        LobbyId = new SteamId { Value = lobbyId };
        PlayerId = new SteamId { Value = playerId };
        
        AuthKey = socketMessage.Read<string>();
    }
}