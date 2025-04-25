using System;
using RepoSteamNetworking.Networking.NetworkedProperties;

namespace RepoSteamNetworking.API.Unity;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class NetworkedPropertyAttribute : Attribute
{
    /// <summary>
    /// Overrides the name of the generated property.
    /// If left unset, the property name will be <c>PascalCase</c> of the target field.
    /// <remarks>Only used when Attribute is applied on a field.</remarks>
    /// </summary>
    public string OverridePropertyName { get; set; } = string.Empty;

    /// <summary>
    /// Overrides the name of the generated backing field.
    /// If left unset, the backing field name will be <c>_lowerCamelCase</c> of the target property.
    /// <remarks>Only used when the attribute is applied on a property.</remarks>
    /// </summary>
    public string OverrideBackingField { get; set; } = string.Empty;

    /// <summary>
    /// Defines the permissions required for writing to the networked property.
    /// By default, the value is set to <see cref="RepoSteamNetworking.API.NetworkPermission.Host"/>.
    /// </summary>
    public NetworkPermission WritePermissions { get; set; } = NetworkPermission.Host;

    /// <summary>
    /// Specifies the name of a callback method to be invoked when the associated property changes.
    /// </summary>
    public string CallbackMethodName { get; set; } = string.Empty;

    /// <summary>
    /// Determines the method used for sending updates to the property across the network.
    /// <remarks>Deltas are not properly implemented, best to leave this unchanged for now.</remarks>
    /// </summary>
    public VariableChangeKind SendMethod { get; set; } = VariableChangeKind.Set;
}