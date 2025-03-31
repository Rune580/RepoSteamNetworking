using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RepoSteamNetworking.Prefab;

internal class PrefabGameObjectState
{
    public readonly string PrefabPath;
    public readonly PrefabComponentState[] Components;
    public readonly PrefabGameObjectState[] Children;

    private PrefabState _prefab;
    
    private readonly Dictionary<string, int> _pathChildMap = new();
    private readonly Dictionary<string, int> _pathComponentMap = new();

    public PrefabGameObjectState(string currentPath, GameObject gameObject, PrefabState prefab)
    {
        _prefab = prefab;
        PrefabPath = string.IsNullOrEmpty(currentPath) ? gameObject.name : $"{currentPath}/{gameObject.name}";

        var components = gameObject.GetComponents<Component>();
        Components = new PrefabComponentState[components.Length];

        var componentIndices = new Dictionary<string, uint>();

        for (var i = 0; i < components.Length; i++)
        {
            var component = components[i];

            var prefabComponent = new PrefabComponentState(component);

            if (!componentIndices.TryGetValue(prefabComponent.FullTypeName, out uint componentIndex))
                componentIndices[prefabComponent.FullTypeName] = componentIndex = 0;
            
            prefabComponent.ComponentIndex = componentIndex++;
            componentIndices[prefabComponent.FullTypeName] = componentIndex;

            var componentPath = $"{PrefabPath}:{prefabComponent.FullTypeName}-{componentIndex}";
            _prefab.AddComponentPath(componentPath);
            _pathComponentMap[componentPath] = i;
            
            Components[i] = prefabComponent;
        }

        var children = new List<PrefabGameObjectState>();
        var childIndex = 0;
        foreach (Transform child in gameObject.transform)
        {
            var prefabGameObject = new PrefabGameObjectState(PrefabPath, child.gameObject, _prefab);
            children.Add(prefabGameObject);
            
            _prefab.AddChildPath(prefabGameObject.PrefabPath);
            _pathChildMap[prefabGameObject.PrefabPath] = childIndex++;
        }
        Children = children.ToArray();
    }

    public bool TryGetChild(string path, out PrefabGameObjectState child)
    {
        child = null!;
        if (!_pathChildMap.TryGetValue(path, out var childIndex))
            return false;
        
        child = Children[childIndex];
        return true;
    }

    public bool TryGetComponent(string componentPath, out PrefabComponentState component)
    {
        component = null!;

        var prefabPath = componentPath.Split(':').First();
        if (PrefabPath != prefabPath)
        {
            if (!TryGetChild(prefabPath, out var child))
                return false;

            return child.TryGetComponent(componentPath, out component);
        }
        
        if (!_pathComponentMap.TryGetValue(componentPath, out var componentIndex))
            return false;
        
        component = Components[componentIndex];
        return true;
    }
}