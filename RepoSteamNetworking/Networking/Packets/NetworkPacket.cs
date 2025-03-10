using System;
using RepoSteamNetworking.Networking.Data;

namespace RepoSteamNetworking.Networking.Packets;

public abstract class NetworkPacket<TPacket> : NetworkPacket
    where TPacket : NetworkPacket<TPacket>
{
    private static Action<TPacket>? _callback;

    internal void RegisterCallback(Action<TPacket> callback)
    {
        _callback = callback;
    }

    internal override void InvokeCallback()
    {
        var packetCasted = (TPacket)this;
        
        _callback?.Invoke(packetCasted);
    }
} 

public abstract class NetworkPacket
{
    internal SocketMessage Serialize(NetworkDestination destination)
    {
        var message = new SocketMessage();

        var packetId = NetworkPacketRegistry.GetPacketId(GetType());

        message.WriteInt(packetId);
        message.WriteByte((byte)destination);
        
        WriteData(message);
        
        return message;
    }

    internal NetworkPacket Deserialize(SocketMessage message)
    {
        ReadData(message);
        
        return this;
    }
    
    /// <summary>
    /// Write data into socket message that you want to send.
    /// The order you write the data matters!
    /// </summary>
    /// <param name="socketMessage"></param>
    protected abstract void WriteData(SocketMessage socketMessage);
    
    /// <summary>
    /// Read data from socket message.
    /// The order you read the data matters!
    /// </summary>
    /// <param name="socketMessage"></param>
    protected abstract void ReadData(SocketMessage socketMessage);

    internal abstract void InvokeCallback();
}