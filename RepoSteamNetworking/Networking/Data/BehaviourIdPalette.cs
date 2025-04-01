using System;
using System.Collections;
using System.Collections.Generic;

namespace RepoSteamNetworking.Networking.Data;

[Serializable]
internal class BehaviourIdPalette : IEnumerable<KeyValuePair<uint, string>>
{
    public Dictionary<uint, string> BehaviourIdToClassName;
    public Dictionary<string, uint> ClassNameToBehaviourId;
    
    public BehaviourIdPalette()
    {
        BehaviourIdToClassName = new Dictionary<uint, string>();
        ClassNameToBehaviourId = new Dictionary<string, uint>();
    }

    public BehaviourIdPalette(IEnumerable<string> behaviourClassNames)
    {
        BehaviourIdToClassName = new Dictionary<uint, string>();
        ClassNameToBehaviourId = new Dictionary<string, uint>();
        
        uint i = 0;
        foreach (var behaviourClassName in behaviourClassNames)
        {
            BehaviourIdToClassName[i] = behaviourClassName;
            ClassNameToBehaviourId[behaviourClassName] = i;
            i++;
        }
    }

    public string GetClassName(uint behaviourId) => BehaviourIdToClassName[behaviourId];
    
    public uint GetBehaviourId(string className) => ClassNameToBehaviourId[className];
    
    public IEnumerator<KeyValuePair<uint, string>> GetEnumerator() => BehaviourIdToClassName.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}