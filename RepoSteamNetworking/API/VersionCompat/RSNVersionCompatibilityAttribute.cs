using System;

namespace RepoSteamNetworking.API.VersionCompat;

[AttributeUsage(AttributeTargets.Class)]
public class RSNVersionCompatibilityAttribute(VersionCompatibility compatibility = VersionCompatibility.Strict, bool optional = false) : Attribute
{
    internal VersionCompatibility Compatibility { get; } = compatibility;
    internal bool Optional { get; } = optional;
}