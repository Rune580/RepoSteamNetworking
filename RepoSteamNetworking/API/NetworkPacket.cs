using RepoSteamNetworking.Networking;
using RepoSteamNetworking.Networking.Packets;
using RepoSteamNetworking.Networking.Registries;
using RepoSteamNetworking.Networking.Serialization;
using Steamworks;

namespace RepoSteamNetworking.API;

public abstract class NetworkPacket<TPacket> : NetworkPacket
    where TPacket : NetworkPacket<TPacket>
{
    /// <summary>
    /// Sets the target of this packet to be the SteamId of a user.
    /// Only used when <see cref="NetworkDestination"/> is <see cref="NetworkDestination.PacketTarget"/>
    /// </summary>
    public TPacket SetTarget(SteamId target)
    {
        Header.Target = target;
        return (TPacket)this;
    }

    /// <summary>
    /// Sets the target of this packet to the SteamId of the sender.
    /// Only used when <see cref="NetworkDestination"/> is <see cref="NetworkDestination.PacketTarget"/>
    /// </summary>
    public TPacket SetTargetToSender()
    {
        Header.Target = Header.Sender;
        return (TPacket)this;
    }
} 

public abstract class NetworkPacket
{
    /// <summary>
    /// Represents the header of a network packet, containing metadata about the packet's
    /// id, destination, sender, and target.
    /// </summary>
    public PacketHeader Header;

    internal SocketMessage Serialize(NetworkDestination destination)
    {
        var message = new SocketMessage();
        
        Header.PacketId = NetworkPacketRegistry.GetPacketId(GetType());
        Header.Destination = destination;

        message.WritePacketHeader(Header);
        
        WriteData(message);
        
        return message;
    }

    internal NetworkPacket Deserialize(SocketMessage message)
    {
        ReadData(message);
        
        return this;
    }
    
    /// <summary>
    /// Write data into a socket message that you want to send.
    /// The order you write the data matters!
    /// </summary>
    /// <param name="socketMessage"></param>
    protected abstract void WriteData(SocketMessage socketMessage);
    
    /// <summary>
    /// Read data from a socket message.
    /// The order you read the data matters!
    /// </summary>
    /// <param name="socketMessage"></param>
    protected abstract void ReadData(SocketMessage socketMessage);
}