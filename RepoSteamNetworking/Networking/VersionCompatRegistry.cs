using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using RepoSteamNetworking.API.VersionCompat;
using RepoSteamNetworking.Networking.Packets;
using RepoSteamNetworking.Utils;

namespace RepoSteamNetworking.Networking;

internal static class VersionCompatRegistry
{
    private static readonly Dictionary<string, RSNVersionCompatibilityAttribute> ModCompatInfo = new();
    private static readonly Dictionary<string, System.Version> ModVersion = new();
    private static bool _assembliesRegistered;
    
    public static void InitRegistry()
    {
        RegisterAssemblies();
    }

    public static void RegisterMod(string modGuid, System.Version modVersion, RSNVersionCompatibilityAttribute compatAttr)
    {
        ModCompatInfo[modGuid] = compatAttr;
        ModVersion[modGuid] = modVersion;
    }

    public static bool IsCompatible(string modGuid, System.Version otherModVersion, out System.Version? serverModVersion, out VersionCompatibility serverCompatibility)
    {
        serverModVersion = null;
        serverCompatibility = default;
        
        if (!ModCompatInfo.TryGetValue(modGuid, out var compatInfo))
        {
            Logging.Warn($"{modGuid} is not registered in VersionCompatRegistry!");
            return false;
        }
        
        serverCompatibility = compatInfo.Compatibility;
        
        if (!ModVersion.TryGetValue(modGuid, out var modVersion))
        {
            Logging.Warn($"{modGuid} is not registered in VersionCompatRegistry!");
            return false;
        }
        
        serverModVersion = modVersion;

        return compatInfo.Compatibility switch
        {
            VersionCompatibility.Strict => modVersion == otherModVersion,
            VersionCompatibility.Any => true,
            VersionCompatibility.Minor => modVersion.Major == otherModVersion.Major,
            VersionCompatibility.Patch => modVersion.Major == otherModVersion.Major && modVersion.Minor == otherModVersion.Minor,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public static bool TryGetVersion(string modGuid, out System.Version version) =>
        ModVersion.TryGetValue(modGuid, out version);

    public static bool TryGetCompatInfo(string modGuid, out RSNVersionCompatibilityAttribute compatInfo) =>
        ModCompatInfo.TryGetValue(modGuid, out compatInfo);
    
    public static bool ContainsEntry(string modGuid) => ModCompatInfo.ContainsKey(modGuid) && ModVersion.ContainsKey(modGuid);

    public static bool IsOptional(string modGuid)
    {
        if (!ModCompatInfo.TryGetValue(modGuid, out var compatInfo))
            return false;

        return compatInfo.Optional;
    }
    
    public static string[] ModGuids => ModCompatInfo.Keys.ToArray();

    public static ClientModVersionRegistryPacket CreateRegistryPacket()
    {
        var packet = new ClientModVersionRegistryPacket
        {
            ModVersions = ModVersion
        };
        
        return packet;
    }

    private static void RegisterAssemblies()
    {
        if (_assembliesRegistered)
            return;
        
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in assembly.GetLoadableTypes())
            {
                var infoAttributes = type.GetCustomAttributes<BepInPlugin>().ToArray();
                if (infoAttributes.Length != 1)
                    continue;
                
                var info = infoAttributes[0];

                var attributes = type.GetCustomAttributes<RSNVersionCompatibilityAttribute>().ToArray();
                if (attributes.Length != 1)
                {
                    // RegisterMod(info.GUID, info.Version, new RSNVersionCompatibilityAttribute());
                    continue;
                }

                var compatibilityAttribute = attributes[0];
                
                RegisterMod(info.GUID, info.Version, compatibilityAttribute);
            }
        }
        
        _assembliesRegistered = true;
    }
}