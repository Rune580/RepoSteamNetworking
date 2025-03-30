using RepoSteamNetworking.API;
using RepoSteamNetworking.Networking.Serialization;

namespace RepoSteamNetworking.Networking.Packets;

internal class CallRPCPacket : NetworkPacket<CallRPCPacket>
{
    public uint NetworkId { get; set; }
    public uint GuidPaletteId { get; set; }
    public uint SubId { get; set; }
    public string MethodName { get; set; } = string.Empty;
    public object[] Parameters { get; set; } = [];
    
    protected override void WriteData(SocketMessage socketMessage)
    {
        socketMessage.Write(NetworkId);
        socketMessage.Write(GuidPaletteId);
        socketMessage.Write(SubId);
        socketMessage.Write(MethodName);
        socketMessage.Write(Parameters);
    }

    protected override void ReadData(SocketMessage socketMessage)
    {
        NetworkId = socketMessage.Read<uint>();
        GuidPaletteId = socketMessage.Read<uint>();
        SubId = socketMessage.Read<uint>();
        MethodName = socketMessage.Read<string>();
        Parameters = socketMessage.Read<object[]>();
    }
}