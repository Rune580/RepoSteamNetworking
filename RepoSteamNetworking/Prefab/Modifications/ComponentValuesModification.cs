using System;
using System.Collections.Generic;
using System.Linq;
using RepoSteamNetworking.Utils;
using UnityEngine;

namespace RepoSteamNetworking.Prefab.Modifications;

[Serializable]
public class ComponentValuesModification : BasePrefabModification
{
    public string ComponentPath;
    public Dictionary<string, object?> Values;
    
    public string FullTypeName => ComponentPath.Split(':').Last().Split('-').First();
    public string PrefabPath => ComponentPath.Split(':').First();

    public ComponentValuesModification()
    {
        ComponentPath = string.Empty;
        Values = new Dictionary<string, object?>();
    }

    public ComponentValuesModification(string componentPath, Dictionary<string, object?> values)
    {
        ComponentPath = componentPath;
        Values = values;
    }

    internal override void Apply(GameObject prefabRoot)
    {
        var child = PrefabPath == prefabRoot.name ? prefabRoot.transform : prefabRoot.transform.Find(PrefabPath);

        var component = child.GetOrAddComponent(FullTypeName);
        component.SetFieldValues(Values);
    }
}