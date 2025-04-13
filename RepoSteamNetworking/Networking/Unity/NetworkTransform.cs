using RepoSteamNetworking.API;
using RepoSteamNetworking.API.Unity;
using RepoSteamNetworking.Networking.Data;
using UnityEngine;

namespace RepoSteamNetworking.Networking.Unity;

[RequireComponent(typeof(Transform))]
[DisallowMultipleComponent]
public partial class NetworkTransform : MonoBehaviour
{
    [NetworkedProperty(CallbackMethodName = nameof(OnReceivedDelta))]
    public partial NetworkTransformDelta TransformDelta { get; set; }
    
    public bool syncPositionX = true;
    public bool syncPositionY = true;
    public bool syncPositionZ = true;
    
    public bool syncRotationX = true;
    public bool syncRotationY = true;
    public bool syncRotationZ = true;
    
    public bool syncScaleX = true;
    public bool syncScaleY = true;
    public bool syncScaleZ = true;
    
    public float positionThreshold = 0.001f;
    public float rotationThreshold = 0.01f;
    public float scaleThreshold = 0.01f;
    
    public int ticksPerSecond = 30;

    private Vector3 PositionMask => new(syncPositionX ? 1 : 0, syncPositionY ? 1 : 0, syncPositionZ ? 1 : 0);
    private Vector3 RotationMask => new(syncRotationX ? 1 : 0, syncRotationY ? 1 : 0, syncRotationZ ? 1 : 0);
    private Vector3 ScaleMask => new(syncScaleX ? 1 : 0, syncScaleY ? 1 : 0, syncScaleZ ? 1 : 0);
    
    private double TimePerTick => 1f / ticksPerSecond;
    private double _timer;
    
    private Vector3 _lastValidPosition;
    private Vector3 _lastValidRotation;
    private Vector3 _lastValidScale;
    
    private void Awake()
    {
        _lastValidPosition = transform.position;
        _lastValidRotation = transform.eulerAngles;
        _lastValidScale = transform.localScale;
    }

    private void Update()
    {
        if (!RepoSteamNetwork.IsServer)
            return;
        
        _timer += Time.deltaTime;

        if (_timer >= TimePerTick)
        {
            TrySyncProperties();
            _timer -= TimePerTick;
        }
    }

    private void TrySyncProperties()
    {
        var position = transform.position;
        if (VectorsChanged(position, _lastValidPosition, PositionMask, positionThreshold))
        {
            _lastValidPosition = position;
            
            if (syncPositionX)
                TransformDelta = TransformDelta with { PositionX = position.x };
            if (syncPositionY)
                TransformDelta = TransformDelta with { PositionY = position.y };
            if (syncPositionZ)
                TransformDelta = TransformDelta with { PositionZ = position.z };
        }
        
        var rotation = transform.eulerAngles;
        if (VectorsChanged(rotation, _lastValidRotation, RotationMask, rotationThreshold))
        {
            _lastValidRotation = rotation;
            
            if (syncRotationX)
                TransformDelta = TransformDelta with { RotationX = rotation.x };
            if (syncRotationY)
                TransformDelta = TransformDelta with { RotationY = rotation.y };
            if (syncRotationZ)
                TransformDelta = TransformDelta with { RotationZ = rotation.z };
        }
        
        var scale = transform.localScale;
        if (VectorsChanged(scale, _lastValidScale, ScaleMask, scaleThreshold))
        {
            _lastValidScale = scale;
            
            if (syncScaleX)
                TransformDelta = TransformDelta with { ScaleX = scale.x };
            if (syncScaleY)
                TransformDelta = TransformDelta with { ScaleY = scale.y };
            if (syncScaleZ)
                TransformDelta = TransformDelta with { ScaleZ = scale.z };
        }
    }

    private void OnReceivedDelta(NetworkTransformDelta delta)
    {
        if (RepoSteamNetwork.IsServer)
            return;
        
        Debug.Log($"Received Delta {delta.PositionX}, {delta.PositionY}, {delta.PositionZ}");
        
        var position = transform.position;
        if (delta.PositionX.HasValue)
            position.x = delta.PositionX.Value;
        if (delta.PositionY.HasValue)
            position.y = delta.PositionY.Value;
        if (delta.PositionZ.HasValue)
            position.z = delta.PositionZ.Value;
        transform.position = position;
        
        var rotation = transform.eulerAngles;
        if (delta.RotationX.HasValue)
            rotation.x = delta.RotationX.Value;
        if (delta.RotationY.HasValue)
            rotation.y = delta.RotationY.Value;
        if (delta.RotationZ.HasValue)
            rotation.z = delta.RotationZ.Value;
        transform.eulerAngles = rotation;
        
        var scale = transform.localScale;
        if (delta.ScaleX.HasValue)
            scale.x = delta.ScaleX.Value;
        if (delta.ScaleY.HasValue)
            scale.y = delta.ScaleY.Value;
        if (delta.ScaleZ.HasValue)
            scale.z = delta.ScaleZ.Value;
        transform.localScale = scale;
    }

    private static Vector3 MaskVector(Vector3 vector, Vector3 mask) =>
        new(vector.x * mask.x, vector.y * mask.y, vector.z * mask.z);

    private static bool VectorsChanged(Vector3 left, Vector3 right, Vector3 mask, float threshold) =>
        Vector3.Distance(MaskVector(left, mask), MaskVector(right, mask)) > threshold;
}