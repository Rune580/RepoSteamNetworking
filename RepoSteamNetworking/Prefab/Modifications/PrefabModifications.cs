using System.Collections.Generic;
using UnityEngine;

namespace RepoSteamNetworking.Prefab.Modifications;

internal class PrefabModifications
{
    private readonly List<BasePrefabModification> _modifications = [];
    
    public BasePrefabModification[] AsArray() => _modifications.ToArray();
    
    public PrefabModifications() { }

    public PrefabModifications(BasePrefabModification[] modifications)
    {
        _modifications = new List<BasePrefabModification>(modifications);
    }

    public void AddModification(BasePrefabModification modification)
    {
        _modifications.Add(modification);
    }

    public void ApplyModifications(GameObject prefabRoot)
    {
        foreach (var modification in _modifications)
            modification.Apply(prefabRoot);
    }
}