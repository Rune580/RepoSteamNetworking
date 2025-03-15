using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RepoSteamNetworking.Utils;

internal static class AssemblyUtils
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
}