using System;

namespace RepoSteamNetworking.Prefab.Modifications;

[Serializable]
public class GameObjectAddedModification
{
    public string Name;
    public string ParentPrefabPath;
    
    public BasePrefabModification[] Modifications;
}