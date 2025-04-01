using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace RepoSteamNetworking.Utils;

internal static class ComponentUtils
{
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;

    public static IEnumerable<FieldInfo> GetAllSerializedFields(this Component component)
    {
        var componentType = component.GetType();
        return componentType.GetFields(Flags)
            .Where(type => type.IsDefined(typeof(SerializeField), false) || type.IsPublic);
    }

    public static Component GetOrAddComponent(this GameObject gameObject, string assemblyQualifiedName)
    {
        var componentType = Type.GetType(assemblyQualifiedName);
        
        var component = gameObject.GetComponent(componentType);
        return !component ? gameObject.AddComponent(componentType) : component;
    }

    public static Component GetOrAddComponent(this Transform transform, string assemblyQualifiedName)
    {
        return transform.gameObject.GetOrAddComponent(assemblyQualifiedName);
    }

    public static void SetFieldValues(this Component component, Dictionary<string, object?> values)
    {
        var componentType = component.GetType();
        
        foreach (var (fieldName, value) in values)
        {
            var field = componentType.GetField(fieldName, Flags);

            if (field is null)
            {
                Logging.Warn($"Failed to find field {fieldName} on component {componentType.Name}!");
                continue;
            }
            
            field.SetValue(component, value);
        }
    }
}