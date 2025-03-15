using Steamworks;

namespace RepoSteamNetworking.Networking.Packets;

public struct PacketHeader
{
    public int PacketId;
    public NetworkDestination Destination;
    public SteamId Sender;
    public SteamId Target;
}