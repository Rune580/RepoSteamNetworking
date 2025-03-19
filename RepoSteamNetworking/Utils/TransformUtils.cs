using UnityEngine;

namespace RepoSteamNetworking.Utils;

internal static class TransformUtils
{
    public static string GetPathFromParent(this Transform current, Transform target)
    {
        var path = current.name;
        
        var parent = current.parent;

        while (parent != target && parent)
        {
            path = $"{parent.name}/{path}";
            parent = parent.parent;
        }
        
        return path;
    }
}