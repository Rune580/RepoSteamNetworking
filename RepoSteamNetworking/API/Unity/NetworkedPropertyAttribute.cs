using System;
using RepoSteamNetworking.Networking.NetworkedProperties;

namespace RepoSteamNetworking.API.Unity;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class NetworkedPropertyAttribute : Attribute
{
    public string OverridePropertyName { get; set; } = string.Empty;
    
    public string OverrideBackingField { get; set; } = string.Empty;
    
    public string CallbackMethodName { get; set; } = string.Empty;
    
    public VariableChangeKind SendMethod { get; set; } = VariableChangeKind.Set;
}