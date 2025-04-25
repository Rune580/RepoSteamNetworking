using System;

namespace RepoSteamNetworking.API.Unity;

/// <summary>
/// An attribute used to designate a method as an RPC.
/// This allows for the source generator to create the required to code to properly network this method.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class RepoSteamRPCAttribute(RPCTarget target = RPCTarget.Everyone) : Attribute
{
    /// <summary>
    /// Gets the target type for the RPC method.
    /// The target can specify where the RPC is directed, for instance, to the host, clients, or everyone.
    /// </summary>
    public RPCTarget Target { get; private set; } = target;
}