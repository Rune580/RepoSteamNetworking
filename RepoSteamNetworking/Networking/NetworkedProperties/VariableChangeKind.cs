using System;

namespace RepoSteamNetworking.Networking.NetworkedProperties;

public enum VariableChangeKind : byte
{
    Set,
    /// <summary>
    /// Delta of the variables value
    /// <remarks>Not properly implemented</remarks>
    /// </summary>
    [Obsolete("Not properly implemented, do not use!")]
    Delta
}