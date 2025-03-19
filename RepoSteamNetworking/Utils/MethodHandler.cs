using System;
using System.Reflection;

namespace RepoSteamNetworking.Utils;

internal class MethodHandler : IEquatable<MethodHandler>
{
    private readonly MethodInfo _method;
    private readonly object? _target;

    private MethodHandler(MethodInfo method, object? target = null)
    {
        _method = method;
        _target = target;
    }

    public void Invoke(params object[] parameters)
    {
        _method.Invoke(_target, parameters);
    }

    public static MethodHandler FromAction<T1>(Action<T1> action)
    {
        var method = action.Method;
        var target = action.Target;

        return new MethodHandler(method, target);
    }

    #region Equality

    public bool Equals(MethodHandler? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;

        if (_method.Name != other._method.Name)
            return false;

        if (_target != other._target)
            return false;
        
        var parameters = _method.GetParameters();
        var otherParameters = other._method.GetParameters();
        
        if (parameters.Length != otherParameters.Length)
            return false;

        for (var i = 0; i < parameters.Length; i++)
        {
            if (parameters[i] != otherParameters[i])
                return false;
        }
        
        return true;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != GetType())
            return false;
        return Equals((MethodHandler)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_method, _target);
    }

    public static bool operator ==(MethodHandler? left, MethodHandler? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(MethodHandler? left, MethodHandler? right)
    {
        return !Equals(left, right);
    }

    #endregion
}