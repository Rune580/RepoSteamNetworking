using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RepoSteamNetworking.Utils;

internal class RPCMethodHelper
{
    private readonly Dictionary<int, Dictionary<int, MethodInfo>> _methodInfoCache = new();
    
    public void InvokeRPC(object target, string methodName, object[] parameters)
    {
        var methodInfo = GetMethodInfo(target, methodName, parameters);
        methodInfo?.Invoke(target, parameters);
    }

    private MethodInfo? GetMethodInfo(object target, string methodName, object[] parameters)
    {
        var targetHashcode = GetHashcodeFromTarget(target);
        if (!_methodInfoCache.TryGetValue(targetHashcode, out var cache))
            _methodInfoCache[targetHashcode] = cache = new Dictionary<int, MethodInfo>();
        
        var parameterTypes = parameters.Select(parameter => parameter.GetType())
            .ToArray();

        var methodHashcode = GetHashcodeFromMethod(methodName, parameterTypes);
        if (!cache.TryGetValue(methodHashcode, out var info))
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            
            cache[methodHashcode] = info = target.GetType().GetMethod(
                methodName,
                flags,
                null,
                CallingConventions.Any,
                parameterTypes,
                []
            );
        }

        if (info is null)
        {
            Logging.Error($"Failed to find RPC method {methodName} on {target.GetType().Name} with parameter types {parameterTypes.Select(type => type.FullName).ToArray().DebugFormatArray()}");
            return null;
        }
        
        return info;
    }

    private static int GetHashcodeFromTarget(object target)
    {
        var type = target.GetType();
        return $"{type.Assembly.FullName}{type.FullName}".GetHashCode();
    }

    private static int GetHashcodeFromMethod(string methodName, Type[] parameterTypes)
    {
        var signature = $"{methodName},";

        signature = parameterTypes.Aggregate(signature, (current, parameterType) => current + $"{parameterType.Assembly.FullName}{parameterType.FullName},");
        signature = signature.TrimEnd(',');

        return signature.GetHashCode();
    }
}