using System;

namespace RepoSteamNetworking.Prefab.Modifications;

[Serializable]
public class ComponentRemovedModification : BasePrefabModification
{
    public string ComponentPath;
}