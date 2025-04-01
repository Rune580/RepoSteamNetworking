using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RepoSteamNetworking.Prefab.Modifications;

[Serializable]
public class GameObjectRemovedModification : BasePrefabModification
{
    public string PrefabPath;

    public GameObjectRemovedModification()
    {
        PrefabPath = string.Empty;
    }

    public GameObjectRemovedModification(string prefabPath)
    {
        PrefabPath = prefabPath;
    }

    internal override void Apply(GameObject prefabRoot)
    {
        var child = PrefabPath == prefabRoot.name ? prefabRoot.transform : prefabRoot.transform.Find(PrefabPath);
        Object.DestroyImmediate(child);
    }
}