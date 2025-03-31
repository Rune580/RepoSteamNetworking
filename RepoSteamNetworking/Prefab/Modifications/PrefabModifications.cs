using System.Collections.Generic;

namespace RepoSteamNetworking.Prefab.Modifications;

internal class PrefabModifications
{
    private readonly List<BasePrefabModification> _modifications = [];
    
    public PrefabModifications() { }

    public PrefabModifications(BasePrefabModification[] modifications)
    {
        _modifications = new List<BasePrefabModification>(modifications);
    }

    public void AddModification(BasePrefabModification modification)
    {
        _modifications.Add(modification);
    }
}