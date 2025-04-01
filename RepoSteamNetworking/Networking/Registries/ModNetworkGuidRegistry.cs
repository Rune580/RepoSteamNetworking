using System.Linq;
using BepInEx.Bootstrap;
using RepoSteamNetworking.Networking.Data;

namespace RepoSteamNetworking.Networking.Registries;

internal static class ModNetworkGuidRegistry
{
    public static ModNetworkGuidPalette Palette { get; private set; } = new();
    private static bool _initialized;

    public static void Init()
    {
        if (_initialized)
            return;

        var modGuids = Chainloader.PluginInfos.Keys.ToArray();
        Palette = new ModNetworkGuidPalette(modGuids);
        
        _initialized = true;
    }
}