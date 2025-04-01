using System;
using System.Collections.Generic;
using System.Reflection;
using RepoSteamNetworking.Networking.Attributes;
using RepoSteamNetworking.Networking.Data;
using RepoSteamNetworking.Utils;

namespace RepoSteamNetworking.Networking.Registries;

internal static class BehaviourIdRegistry
{
    public static BehaviourIdPalette Palette { get; private set; } = new();
    private static bool _initialized;
    
    public static void Init()
    {
        if (_initialized)
            return;

        var classNames = new HashSet<string>();

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in assembly.GetLoadableTypes())
            {
                var generateBehaviourIdAttr = type.GetCustomAttribute<GenerateBehaviourIdAttribute>();
                if (generateBehaviourIdAttr is null)
                    continue;
                
                classNames.Add(type.FullName);
            }
        }
        
        Palette = new BehaviourIdPalette(classNames);
        
        _initialized = true;
    }
}