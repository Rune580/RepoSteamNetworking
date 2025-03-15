using System;
using System.Collections.Generic;
using System.Reflection;
using RepoSteamNetworking.API;
using RepoSteamNetworking.Utils;
using RepoSteamNetworking.Utils.Reflection;

namespace RepoSteamNetworking.Networking.Packets;

internal static class NetworkPacketRegistry
{
    private static readonly Dictionary<int, Type> PacketTypes = new();

    private static readonly Dictionary<int, HashSet<MethodHandler>> PacketCallbacks = new();
 
    public static void RegisterPacket(Type packetType)
    {
        var packetFullName = packetType.FullName;
        if (string.IsNullOrEmpty(packetFullName))
            throw new InvalidOperationException("Cannot register packets without a packet type.");

        var packetId = GetPacketId(packetType);
        
        PacketTypes[packetId] = packetType;
        PacketCallbacks[packetId] = [];
    }

    public static int GetPacketId(Type packetType) =>
        $"{packetType.Assembly.FullName}{packetType.FullName}".GetHashCode();

    public static NetworkPacket CreatePacket(int packetId)
    {
        var packetType = PacketTypes[packetId];
        var packet = Activator.CreateInstance(packetType);

        return (NetworkPacket)packet;
    }

    public static void AddCallback<TPacket>(MethodHandler methodHandler)
    {
        var packetId = GetPacketId(typeof(TPacket));

        if (!PacketCallbacks.TryGetValue(packetId, out var callbacks))
        {
            Logging.Warn($"Cannot add callback to packet {typeof(TPacket).Name}!, are you sure it's registered?");
            return;
        }
        
        callbacks.Add(methodHandler);
    }

    public static void RemoveCallback<TPacket>(MethodHandler methodHandler)
    {
        var packetId = GetPacketId(typeof(TPacket));
        
        if (!PacketCallbacks.TryGetValue(packetId, out var callbacks))
        {
            Logging.Warn($"Cannot remove callback from packet {typeof(TPacket).Name}!, are you sure it's registered?");
            return;
        }
        
        callbacks.Remove(methodHandler);
    }

    public static void InvokeCallbacks(NetworkPacket packet)
    {
        var packetId = packet.Header.PacketId;

        if (!PacketCallbacks.TryGetValue(packetId, out var callbacks))
        {
            Logging.Warn($"Packet {packet.GetType().Name} isn't registered!");
            return;
        }
        
        foreach (var callback in callbacks)
        {
            callback.Invoke(packet);
        }
    }
}