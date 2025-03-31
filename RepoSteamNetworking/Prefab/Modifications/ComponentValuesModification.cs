using System;
using System.Collections.Generic;

namespace RepoSteamNetworking.Prefab.Modifications;

[Serializable]
public class ComponentValuesModification : BasePrefabModification
{
    public string ComponentPath;
    public Dictionary<string, object?> Values;
}