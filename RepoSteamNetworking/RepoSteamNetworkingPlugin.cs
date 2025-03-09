using System.Reflection;
using BepInEx;
using HarmonyLib;

namespace RepoSteamNetworking;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class RepoSteamNetworkingPlugin : BaseUnityPlugin
{
    public static RepoNetworkSocketManager? SocketManager;
    public static RepoNetworkConnectionManager? ConnectionManager;

    private void Awake()
    {
        Logging.SetLogSource(Logger);
        
        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), PluginInfo.PLUGIN_GUID);
    }
}