using System;
using System.Collections.Generic;
using System.Linq;

namespace RepoSteamNetworking.Prefab.Modifications;

[Serializable]
public class ComponentAddedModification : BasePrefabModification
{
    public string ComponentPath;
    public Dictionary<string, object?> Values;
    
    public string FullTypeName => ComponentPath.Split(':').Last().Split('-').First();
}