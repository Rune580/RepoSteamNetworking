using System;
using UnityEngine;

namespace RepoSteamNetworking.Utils;

internal static class VariableDeltaHelper
{
    public static object Add(object left, object right)
    {
        return left switch
        {
            byte value => value + (byte)right,
            sbyte value => value + (sbyte)right,
            ushort value => value + (ushort)right,
            short value => value + (short)right,
            uint value => value + (uint)right,
            int value => value + (int)right,
            ulong value => value + (ulong)right,
            long value => value + (long)right,
            float value => value + (float)right,
            double value => value + (double)right,
            Vector2 value => value + (Vector2)right,
            Vector2Int value => value + (Vector2Int)right,
            Vector3 value => value + (Vector3)right,
            Vector3Int value => value + (Vector3Int)right,
            Vector4 value => value + (Vector4)right,
            Color value => value + (Color)right,
            _ => throw new ArgumentOutOfRangeException(nameof(left), left, null)
        };
    }
}