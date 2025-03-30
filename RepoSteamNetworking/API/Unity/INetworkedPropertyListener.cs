using RepoSteamNetworking.Networking.NetworkedProperties;

namespace RepoSteamNetworking.API.Unity;

public interface INetworkedPropertyListener
{
    public void OnNetworkedPropertiesDataReceived(string targetClass, NetworkedPropertyChange[] props);
}