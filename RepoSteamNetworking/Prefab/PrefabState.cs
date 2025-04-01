using System.Collections.Generic;
using System.Linq;
using RepoSteamNetworking.Prefab.Modifications;
using RepoSteamNetworking.Utils;
using UnityEngine;

namespace RepoSteamNetworking.Prefab;

internal class PrefabState
{
    public readonly PrefabGameObjectState Root;

    private readonly HashSet<string> _childPaths = [];
    private readonly HashSet<string> _componentPaths = [];

    public PrefabState(GameObject gameObject)
    {
        Root = new PrefabGameObjectState("", gameObject, this);

#if DEBUG
        Logging.Info($"Component Paths: {_componentPaths.DebugFormatArray()}");
#endif
    }

    public bool TryGetGameObject(string path, out PrefabGameObjectState gameObject)
    {
        if (string.IsNullOrEmpty(path) || path == Root.Name)
        {
            gameObject = Root;
            return true;
        }
        
        return Root.TryGetChild(path, out gameObject);
    }

    public void AddChildPath(string path) => _childPaths.Add(path);
    
    public void AddComponentPath(string path) => _componentPaths.Add(path);

    public bool TryGetComponent(string componentPath, out PrefabComponentState component) =>
        Root.TryGetComponent(componentPath, out component);

    public PrefabModifications GetModifications(PrefabState modifiedPrefab)
    {
        var otherChildPaths = new HashSet<string>(modifiedPrefab._childPaths);
        var otherComponentPaths = new HashSet<string>(modifiedPrefab._componentPaths);
        
        var removedChildPaths = new HashSet<string>();
        var removedComponentPaths = new HashSet<string>();
        
        var modifications = new PrefabModifications();

        // Find GameObjects that were removed from the modified prefab
        foreach (var childPath in _childPaths)
        {
#if DEBUG
            Logging.Info($"Checking child path: {childPath}");
#endif  
            
            // Skip child if parent was already removed.
            if (removedChildPaths.Any(removedPath => childPath.Contains(removedPath)))
                continue;

            PrefabGameObjectState otherChild;

            if (childPath == Root.Name)
            {
                otherChild = modifiedPrefab.Root;
            }
            else if (!modifiedPrefab.TryGetGameObject(childPath, out otherChild))
            {
#if DEBUG
                Logging.Info($"Modified prefab is missing child path: {childPath}");
#endif
                
                removedChildPaths.Add(childPath);
                modifications.AddModification(new GameObjectRemovedModification(childPath));
                continue;
            }
            
#if DEBUG
            Logging.Info($"Modified prefab has child path: {childPath}");
#endif
            
            // Check components on modified prefab.
            TryGetGameObject(childPath, out var child);
            foreach (var component in child.Components)
            {
                var componentPath = $"{child.PrefabPath}:{component.FullTypeName}-{component.ComponentIndex}";

                if (!otherChild.TryGetComponent(componentPath, out var otherComponent))
                {
#if DEBUG
                    Logging.Info($"Modified prefab is missing component path: {componentPath}");
#endif
                    
                    removedComponentPaths.Add(componentPath);
                    modifications.AddModification(new ComponentRemovedModification(componentPath));
                    continue;
                }
                
#if DEBUG
                Logging.Info($"Modified prefab has component path: {componentPath}");
#endif
                
                var changedFieldValues = component.GetChangedValues(otherComponent);
                if (changedFieldValues.Count > 0)
                {
                    otherComponentPaths.Remove(componentPath);
                    modifications.AddModification(new ComponentValuesModification(componentPath, changedFieldValues));
                    continue;
                }
                
                otherComponentPaths.Remove(componentPath);
            }

            // Find Components that were added to the child.
            var componentsToAdd = otherComponentPaths.Where(otherComponentPath => !removedComponentPaths.Any(otherComponentPath.Contains));
            foreach (var componentPathToAdd in componentsToAdd)
            {
                if (!otherChild.TryGetComponent(componentPathToAdd, out var componentToAdd))
                {
                    Logging.Error("Failed to find component in Modified Prefab even though path came from said prefab?");
                    continue;
                }
                
#if DEBUG
                Logging.Info($"Modified prefab has additional component path: {componentPathToAdd}");
#endif
                
                modifications.AddModification(new ComponentAddedModification(componentPathToAdd, componentToAdd.Values));
            }

            otherChildPaths.Remove(childPath);
        }

        // Order using depth of child.
        var childPathsToAdd = otherChildPaths.Where(otherChild => !removedChildPaths.Any(otherChild.Contains))
            .OrderBy(childPath => childPath.Split('/').Length);

        var addedChildPaths = new HashSet<string>();

        // Find GameObjects that were added to the modified prefab.
        foreach (var childPath in childPathsToAdd)
        {
            // Skip child if parent was already added.
            if (addedChildPaths.Any(addedPath => childPath.Contains(addedPath)))
                continue;

            if (!modifiedPrefab.TryGetGameObject(childPath, out var otherChild))
            {
                Logging.Error("Failed to find child in Modified Prefab even though path came from said prefab?");
                continue;
            }
            
            modifications.AddModification(new GameObjectAddedModification(otherChild));

            addedChildPaths.Add(childPath);
        }

        return modifications;
    }
}