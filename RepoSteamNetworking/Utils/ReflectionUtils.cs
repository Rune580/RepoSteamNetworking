using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;

namespace RepoSteamNetworking.Utils;

internal static class ReflectionUtils
{
    public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            return e.Types.Where(t => t is not null);
        }
    }

    public static BepInPlugin? GetPluginInfoFromAssembly(this Assembly assembly)
    {
        var pluginInfo = assembly.GetLoadableTypes()
            .SelectMany(type => type.GetCustomAttributes<BepInPlugin>())
            .FirstOrDefault();
        
        return pluginInfo;
    }
}