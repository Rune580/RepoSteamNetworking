using System;
using System.Linq;
using RepoSteamNetworking.API;
using RepoSteamNetworking.Networking.Data;

namespace RepoSteamNetworking.Networking.Packets;

internal class CallRPCPacket : NetworkPacket<CallRPCPacket>
{
    public uint NetworkId { get; set; }
    public int SubId { get; set; }
    public string MethodName { get; set; } = string.Empty;
    public object[] Parameters { get; set; } = [];
    
    protected override void WriteData(SocketMessage socketMessage)
    {
        socketMessage.Write(NetworkId);
        socketMessage.Write(SubId);
        socketMessage.Write(MethodName);
        socketMessage.Write(Parameters.Select(param => param.GetType().AssemblyQualifiedName));
        socketMessage.Write(Parameters);
    }

    protected override void ReadData(SocketMessage socketMessage)
    {
        NetworkId = socketMessage.Read<uint>();
        SubId = socketMessage.Read<int>();
        MethodName = socketMessage.Read<string>();
        
        var parameterTypeNames = socketMessage.Read<string[]>();
        var parameterTypes = parameterTypeNames.Select(Type.GetType);

        Parameters = socketMessage.Read<object[]>();
    }
}