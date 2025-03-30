namespace RepoSteamNetworking.Networking.NetworkedProperties;

public struct NetworkedPropertyChange
{
    public VariableChangeKind ChangeKind;
    public uint PropertyId;
    public object Value;
}