using System;

namespace RepoSteamNetworking.Networking.Data;

public struct NetworkTransformDelta : IEquatable<NetworkTransformDelta>
{
    public float? PositionX;
    public float? PositionY;
    public float? PositionZ;
    
    public float? RotationX;
    public float? RotationY;
    public float? RotationZ;
    
    public float? ScaleX;
    public float? ScaleY;
    public float? ScaleZ;

    public bool Equals(NetworkTransformDelta other)
    {
        return Nullable.Equals(PositionX, other.PositionX) && Nullable.Equals(PositionY, other.PositionY) &&
               Nullable.Equals(PositionZ, other.PositionZ) && Nullable.Equals(RotationX, other.RotationX) &&
               Nullable.Equals(RotationY, other.RotationY) && Nullable.Equals(RotationZ, other.RotationZ) &&
               Nullable.Equals(ScaleX, other.ScaleX) && Nullable.Equals(ScaleY, other.ScaleY) &&
               Nullable.Equals(ScaleZ, other.ScaleZ);
    }

    public override bool Equals(object? obj)
    {
        return obj is NetworkTransformDelta other && Equals(other);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(PositionX);
        hashCode.Add(PositionY);
        hashCode.Add(PositionZ);
        hashCode.Add(RotationX);
        hashCode.Add(RotationY);
        hashCode.Add(RotationZ);
        hashCode.Add(ScaleX);
        hashCode.Add(ScaleY);
        hashCode.Add(ScaleZ);
        return hashCode.ToHashCode();
    }

    public static bool operator ==(NetworkTransformDelta left, NetworkTransformDelta right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(NetworkTransformDelta left, NetworkTransformDelta right)
    {
        return !left.Equals(right);
    }
}