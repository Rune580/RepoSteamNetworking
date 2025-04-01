using System;
using System.Collections;
using System.Collections.Generic;

namespace RepoSteamNetworking.Networking.Data;

[Serializable]
internal class ModNetworkGuidPalette : IEnumerable<KeyValuePair<uint, string>>
{
    public Dictionary<uint, string> ModGuids;
    public Dictionary<string, uint> PaletteIds;

    public ModNetworkGuidPalette()
    {
        ModGuids = new Dictionary<uint, string>();
        PaletteIds = new Dictionary<string, uint>();
    }

    public ModNetworkGuidPalette(string[] guids)
    {
        ModGuids = new Dictionary<uint, string>();
        PaletteIds = new Dictionary<string, uint>();
        
        for (uint i = 0; i < guids.Length; i++)
        {
            ModGuids[i] = guids[i];
            PaletteIds[guids[i]] = i;
        }
    }
    
    public string GetModGuid(uint paletteId) => ModGuids[paletteId];

    public uint GetPaletteId(string modGuid) => PaletteIds[modGuid];
    
    public IEnumerator<KeyValuePair<uint, string>> GetEnumerator() => ModGuids.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}