using RepoSteamNetworking.API.Unity;
using UnityEngine;

namespace RepoSteamNetworking.Networking.Unity;

[RequireComponent(typeof(Transform))]
[DisallowMultipleComponent]
public partial class NetworkTransform : MonoBehaviour
{
    [NetworkedProperty(OverrideBackingField = "transform.position")]
    public partial Vector3 SyncedPosition { get; set; }
    
    [NetworkedProperty(OverrideBackingField = "transform.eulerAngles")]
    public partial Vector3 SyncedRotation { get; set; }
    
    [NetworkedProperty(OverrideBackingField = "transform.localScale")]
    public partial Vector3 SyncedScale { get; set; }
    
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

            var posToSet = SyncedPosition;
            
            if (syncPositionX)
                posToSet.x = position.x;
            if (syncPositionY)
                posToSet.y = position.y;
            if (syncPositionZ)
                posToSet.z = position.z;
            
            SyncedPosition = posToSet;
        }
        
        var rotation = transform.eulerAngles;
        if (VectorsChanged(rotation, _lastValidRotation, RotationMask, rotationThreshold))
        {
            _lastValidRotation = rotation;
            
            var rotToSet = SyncedRotation;
            
            if (syncRotationX)
                rotToSet.x = position.x;
            if (syncRotationY)
                rotToSet.y = position.y;
            if (syncRotationZ)
                rotToSet.z = position.z;
            
            SyncedRotation = rotToSet;
        }
        
        var scale = transform.localScale;
        if (VectorsChanged(scale, _lastValidScale, ScaleMask, scaleThreshold))
        {
            _lastValidScale = scale;
            
            var scaleToSet = SyncedScale;
        
            if (syncScaleX)
                scaleToSet.x = scale.x;
            if (syncScaleY)
                scaleToSet.y = scale.y;
            if (syncScaleZ)
                scaleToSet.z = scale.z;
            
            SyncedScale = scaleToSet;
        }
    }

    private static Vector3 MaskVector(Vector3 vector, Vector3 mask) =>
        new(vector.x * mask.x, vector.y * mask.y, vector.z * mask.z);

    private static bool VectorsChanged(Vector3 left, Vector3 right, Vector3 mask, float threshold) =>
        Vector3.Distance(MaskVector(left, mask), MaskVector(right, mask)) > threshold;
}