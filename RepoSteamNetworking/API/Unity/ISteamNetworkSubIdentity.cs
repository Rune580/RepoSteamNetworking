namespace RepoSteamNetworking.API.Unity;

public interface ISteamNetworkSubIdentity
{
    public uint SubId { get; set; }
    
    public bool IsValid { get; set; }

    public RepoSteamNetworkIdentity GetNetworkIdentity();
}