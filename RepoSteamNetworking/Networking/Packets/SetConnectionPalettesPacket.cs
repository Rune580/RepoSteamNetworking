using RepoSteamNetworking.API;
using RepoSteamNetworking.Networking.Data;
using RepoSteamNetworking.Networking.Serialization;

namespace RepoSteamNetworking.Networking.Packets;

internal class SetConnectionPalettesPacket : NetworkPacket<SetConnectionPalettesPacket>
{
    public ModNetworkGuidPalette GuidPalette { get; set; }
    public BehaviourIdPalette BehaviourIdPalette { get; set; }
    
    protected override void WriteData(SocketMessage socketMessage)
    {
        socketMessage.Write(GuidPalette);
        socketMessage.Write(BehaviourIdPalette);
    }

    protected override void ReadData(SocketMessage socketMessage)
    {
        GuidPalette = socketMessage.Read<ModNetworkGuidPalette>();
        BehaviourIdPalette = socketMessage.Read<BehaviourIdPalette>();
    }
}