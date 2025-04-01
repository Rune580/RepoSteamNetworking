using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RepoSteamNetworking.Utils;
using UnityEngine;

namespace RepoSteamNetworking.Prefab;

internal class PrefabComponentState
{
    public readonly string FullTypeName;
    public readonly FieldInfo[] SerializableFields;
    public readonly Dictionary<string, object?> Values = new();
    public uint ComponentIndex;

    public PrefabComponentState(Component component)
    {
        FullTypeName = component.GetType().AssemblyQualifiedName!;

        var fields = new List<FieldInfo>();

        foreach (var fieldInfo in component.GetAllSerializedFields())
        {
            if (fieldInfo is null)
                continue;
            
            fields.Add(fieldInfo);
            Values[fieldInfo.Name] = fieldInfo.GetValue(component);
        }

        SerializableFields = fields.ToArray();
    }

    public bool AreFieldsEqual(PrefabComponentState other) => GetChangedValues(other).Count == 0;

    public Dictionary<string, object?> GetChangedValues(PrefabComponentState other)
    {
        var diff = new Dictionary<string, object?>();
        
        var otherValueFields = other.Values.Keys.ToHashSet();
        foreach (var (fieldName, value) in Values)
        {
            if (!other.Values.TryGetValue(fieldName, out var otherValue))
            {
                diff[fieldName] = null;
                continue;
            }

            if (value != otherValue)
            {
                diff[fieldName] = otherValue;
                continue;
            }

            if (otherValueFields.Remove(fieldName))
                continue;
            
            diff[fieldName] = null;
        }

        foreach (var otherFieldName in otherValueFields)
            diff[otherFieldName] = other.Values[otherFieldName];
        
        return diff;
    }
}