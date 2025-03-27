using RepoSteamNetworking.API;
using RepoSteamNetworking.Networking.Data;

namespace RepoSteamNetworking.Networking.Packets;

internal class SetConnectionModNetworkGuidPalettePacket : NetworkPacket<SetConnectionModNetworkGuidPalettePacket>
{
    public ModNetworkGuidPalette Palette { get; set; }
    
    protected override void WriteData(SocketMessage socketMessage)
    {
        socketMessage.Write(Palette);
    }

    protected override void ReadData(SocketMessage socketMessage)
    {
        Palette = socketMessage.Read<ModNetworkGuidPalette>();
    }
}