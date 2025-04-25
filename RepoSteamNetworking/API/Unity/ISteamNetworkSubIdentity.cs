namespace RepoSteamNetworking.API.Unity;

public interface ISteamNetworkSubIdentity
{
    /// <summary>
    /// Gets GUID that represents the mod associated with this <see cref="UnityEngine.MonoBehaviour"/>.
    /// This property is used to differentiate between sub-identities that belong to mods.
    /// </summary>
    public string ModGuid { get; }

    /// <summary>
    /// Represents a unique identifier for sub-identities within a mod's scope.
    /// This property is used to differentiate between multiple sub-identities registered under the same mod.
    /// </summary>
    public uint SubId { get; internal set; }

    public bool IsValid { get; set; }

    /// <summary>
    /// Retrieves the network identity associated with the current instance of ISteamNetworkSubIdentity.
    /// </summary>
    /// <returns>
    /// A <see cref="RepoSteamNetworkIdentity"/> object representing the network identity tied to this sub-identity.
    /// </returns>
    public RepoSteamNetworkIdentity GetNetworkIdentity();
}