using System;
using System.Collections.Generic;

namespace RepoSteamNetworking.Networking.Data;

[Serializable]
internal class BehaviourIdPalette
{
    private Dictionary<uint, string> _behaviourIdToClassName = new();
    private Dictionary<string, uint> _classNameToBehaviourId = new();

    public void SetBehaviourNames(IEnumerable<string> behaviourClassNames)
    {
        uint i = 0;
        foreach (var behaviourClassName in behaviourClassNames)
        {
            _behaviourIdToClassName[i] = behaviourClassName;
            _classNameToBehaviourId[behaviourClassName] = i;
            i++;
        }
    }

    public string GetClassName(uint behaviourId) => _behaviourIdToClassName[behaviourId];
    
    public uint GetBehaviourId(string className) => _classNameToBehaviourId[className];
}