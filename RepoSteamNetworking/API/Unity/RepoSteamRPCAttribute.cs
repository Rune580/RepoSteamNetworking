using System;

namespace RepoSteamNetworking.API.Unity;

[AttributeUsage(AttributeTargets.Method)]
public class RepoSteamRPCAttribute(RPCTarget target = RPCTarget.Everyone) : Attribute
{
    public RPCTarget Target { get; private set; } = target;
}