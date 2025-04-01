using System;
using System.Collections.Generic;
using UnityEngine;

namespace RepoSteamNetworking.Prefab.Modifications;

[Serializable]
public class GameObjectAddedModification : BasePrefabModification
{
    public string Name;
    public string ParentPrefabPath;
    
    [SerializeField]
    private BasePrefabModification[] Modifications;

    public GameObjectAddedModification()
    {
        Name = string.Empty;
        ParentPrefabPath = string.Empty;
        Modifications = [];
    }

    internal GameObjectAddedModification(PrefabGameObjectState gameObjectState)
    {
        Name = gameObjectState.Name;
        ParentPrefabPath = gameObjectState.ParentPrefabPath;

        var modifications = new List<BasePrefabModification>();

        foreach (var component in gameObjectState.Components)
        {
            var componentPath = $"{gameObjectState.PrefabPath}:{component.FullTypeName}-{component.ComponentIndex}";
            modifications.Add(new ComponentAddedModification(componentPath, component.Values));
        }

        foreach (var child in gameObjectState.Children)
        {
            modifications.Add(new GameObjectAddedModification(child));
        }
        
        Modifications = modifications.ToArray();
    }

    internal override void Apply(GameObject prefabRoot)
    {
        var parent = ParentPrefabPath == prefabRoot.name ? prefabRoot.transform : prefabRoot.transform.Find(ParentPrefabPath);

        var child = new GameObject(Name);
        child.transform.SetParent(parent.transform);

        foreach (var modification in Modifications)
            modification.Apply(prefabRoot);
    }
}