using System;
using System.Linq;
using RepoSteamNetworking.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RepoSteamNetworking.Prefab.Modifications;

[Serializable]
public class ComponentRemovedModification : BasePrefabModification
{
    public string ComponentPath;
    
    public string FullTypeName => ComponentPath.Split(':').Last().Split('-').First();
    public string PrefabPath => ComponentPath.Split(':').First();

    public ComponentRemovedModification()
    {
        ComponentPath = string.Empty;
    }

    public ComponentRemovedModification(string componentPath)
    {
        ComponentPath = componentPath;
    }

    internal override void Apply(GameObject prefabRoot)
    {
        var child = PrefabPath == prefabRoot.name ? prefabRoot.transform : prefabRoot.transform.Find(PrefabPath);
        var component = child.GetOrAddComponent(FullTypeName);
        
        Object.DestroyImmediate(component);
    }
}