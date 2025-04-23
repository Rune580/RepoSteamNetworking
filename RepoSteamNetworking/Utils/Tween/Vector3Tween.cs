using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI.CoroutineTween;

namespace RepoSteamNetworking.Utils.Tween;

public struct Vector3Tween : ITweenValue
{
    public bool ignoreTimeScale { get; set; }
    public float duration { get; set; }
    public Vector3 StartValue { get; set; }
    public Vector3 TargetValue { get; set; }
    
    private Vector3TweenCallback? _target;

    public void TweenValue(float floatPercentage)
    {
        if (!ValidTarget())
            return;

        var newValue = Vector3.Lerp(StartValue, TargetValue, floatPercentage);
        _target!.Invoke(newValue);
    }

    public void AddOnChangedCallback(UnityAction<Vector3> callback)
    {
        _target ??= new Vector3TweenCallback();
        _target.AddListener(callback);
    }

    public bool ValidTarget()
    {
        return _target is not null;
    }

    private class Vector3TweenCallback : UnityEvent<Vector3>;
}