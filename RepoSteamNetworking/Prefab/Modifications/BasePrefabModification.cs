using System;
using UnityEngine;

namespace RepoSteamNetworking.Prefab.Modifications;

[Serializable]
public abstract class BasePrefabModification
{
    internal abstract void Apply(GameObject prefabRoot);
}