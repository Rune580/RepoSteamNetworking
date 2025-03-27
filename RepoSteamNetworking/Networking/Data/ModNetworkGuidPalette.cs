using System;
using System.Collections.Generic;

namespace RepoSteamNetworking.Networking.Data;

[Serializable]
internal class ModNetworkGuidPalette
{
    private Dictionary<uint, string> _modGuids = new();
    private Dictionary<string, uint> _paletteIds = new();

    public void SetGuids(string[] guids)
    {
        for (uint i = 0; i < guids.Length; i++)
        {
            _modGuids[i] = guids[i];
            _paletteIds[guids[i]] = i;
        }
    }
    
    public string GetModGuid(uint paletteId) => _modGuids[paletteId];

    public uint GetPaletteId(string modGuid) => _paletteIds[modGuid];
}