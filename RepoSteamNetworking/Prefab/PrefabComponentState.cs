using System.Collections.Generic;
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
        FullTypeName = component.GetType().FullName!;

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
}