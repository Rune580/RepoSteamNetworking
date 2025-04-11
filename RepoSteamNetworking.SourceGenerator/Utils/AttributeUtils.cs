using System.Linq;
using Microsoft.CodeAnalysis;

namespace RepoSteamNetworking.SourceGenerator.Utils;

internal static class AttributeUtils
{
    public static T GetNamedArgument<T>(this AttributeData attributeData, string argumentName)
    {
        return attributeData.NamedArguments.Where(kvp => kvp.Key == argumentName && kvp.Value.Value is T)
            .Select(kvp => kvp.Value.Value is T value ? value : default)
            .FirstOrDefault();
    }
}