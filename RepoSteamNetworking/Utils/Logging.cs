using System.Text;
using BepInEx.Logging;

namespace RepoSteamNetworking.Utils;

internal static class Logging
{
    private static ManualLogSource? _logSource;

    internal static void SetLogSource(ManualLogSource logSource)
    {
        _logSource = logSource;
    }

    public static void Error(object data) => Error(data.ToString());
    public static void Warn(object data) => Warn(data.ToString());
    public static void Info(object data) => Info(data.ToString());
    public static void Debug(object data) => Debug(data.ToString());

    public static void Error(string msg)
    {
        if (_logSource is null)
        {
            UnityEngine.Debug.LogError($"[{PluginInfo.PLUGIN_NAME}] [Error] {msg}");
        }
        else
        {
            _logSource.LogError(msg);
        }
    }

    public static void Warn(string msg)
    {
        if (_logSource is null)
        {
            UnityEngine.Debug.LogWarning($"[{PluginInfo.PLUGIN_NAME}] [Warning] {msg}");
        }
        else
        {
            _logSource.LogWarning(msg);
        }
    }

    public static void Info(string msg)
    {
        if (_logSource is null)
        {
            UnityEngine.Debug.Log($"[{PluginInfo.PLUGIN_NAME}] [Info] {msg}");
        }
        else
        {
            _logSource.LogInfo(msg);
        }
    }

    public static void Debug(string msg)
    {
        if (_logSource is null)
        {
            UnityEngine.Debug.Log($"[{PluginInfo.PLUGIN_NAME}] [Debug] {msg}");
        }
        else
        {
            _logSource.LogDebug(msg);
        }
    }

    public static string DebugFormat(this object obj, bool showTypeName = false, bool newLine = false)
    {
        var type = obj.GetType();

        var builder = new StringBuilder();

        if (showTypeName)
            builder.Append($"{type.Name} ");

        builder.Append("( ");

        foreach (var propertyInfo in type.GetProperties())
        {
            if (newLine)
                builder.AppendLine();
            
            var name = propertyInfo.Name;
            var value = propertyInfo.GetValue(obj);
            
            builder.Append($"{name}: {value},");

            if (!newLine)
                builder.Append(" ");
        }
        
        foreach (var fieldInfo in type.GetFields())
        {
            if (newLine)
                builder.AppendLine();
            
            var name = fieldInfo.Name;
            var value = fieldInfo.GetValue(obj);
            
            builder.Append($"{name}: {value},");

            if (!newLine)
                builder.Append(" ");
        }
        
        if (newLine)
            builder.AppendLine();
        
        builder.Append(")");
        
        return builder.ToString();
    }
}