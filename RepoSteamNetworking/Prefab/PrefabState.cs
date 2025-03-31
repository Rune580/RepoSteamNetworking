using System.Collections.Generic;
using System.Linq;
using RepoSteamNetworking.Prefab.Modifications;
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
    }

    public bool TryGetGameObject(string path, out PrefabGameObjectState gameObject)
    {
        if (string.IsNullOrEmpty(path))
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
            // Skip child if parent was already removed.
            if (removedChildPaths.Any(removedPath => childPath.Contains(removedPath)))
                continue;
            
            var existsInModified = modifiedPrefab.TryGetGameObject(childPath, out var otherChild);
            if (!existsInModified)
            {
                TryGetGameObject(childPath, out var child);

                // foreach (var component in child.Components)
                // {
                //     var componentPath = $"{child.PrefabPath}:{component.FullTypeName}-{component.ComponentIndex}";
                //     componentPaths.Remove(componentPath);
                // }

                removedChildPaths.Add(childPath);
                modifications.AddModification(new GameObjectRemovedModification
                {
                    PrefabPath = childPath
                });
                
                continue;
            }

            otherChildPaths.Remove(childPath);
        }

        var childPathsToAdd = otherChildPaths.Where(otherChild => removedChildPaths.Any(otherChild.Contains))
            .OrderBy(childPath => childPath.Split('/').Length);

        var addedChildPaths = new HashSet<string>();

        // Find GameObjects that were added to the modified prefab.
        foreach (var childPath in childPathsToAdd)
        {
            // Skip child if parent was already added.
            if (addedChildPaths.Any(addedPath => childPath.Contains(addedPath)))
                continue;
            
            

            addedChildPaths.Add(childPath);
        }

        return modifications;
    }
}