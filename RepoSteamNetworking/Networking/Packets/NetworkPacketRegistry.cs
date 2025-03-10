using System;
using System.Collections.Generic;

namespace RepoSteamNetworking.Networking.Packets;

internal static class NetworkPacketRegistry
{
    // private static readonly Dictionary<Type, int> PacketIds = new();
    
    private static readonly Dictionary<int, Type> PacketTypes = new();
 
    public static void RegisterPacket(Type packetType)
    {
        var packetFullName = packetType.FullName;
        if (string.IsNullOrEmpty(packetFullName))
            throw new InvalidOperationException("Cannot register packets without a packet type.");

        var packetId = GetPacketId(packetType);
        
        // PacketIds[packetType] = packetId;
        PacketTypes[packetId] = packetType;
    }

    public static int GetPacketId(Type packetType) =>
        $"{packetType.Assembly.FullName}{packetType.FullName}".GetHashCode();

    public static NetworkPacket CreatePacket(int packetId)
    {
        var packetType = PacketTypes[packetId];
        var packet = Activator.CreateInstance(packetType);

        return (NetworkPacket)packet;
    }
}