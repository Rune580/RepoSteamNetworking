using RepoSteamNetworking.API.Unity;
using UnityEngine;

namespace RepoSteamNetworking.Networking.Unity;

[RequireComponent(typeof(Transform))]
[DisallowMultipleComponent]
public partial class NetworkTransform : MonoBehaviour
{
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

        while (_timer >= TimePerTick)
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
            SyncPosition(position, syncPositionX, syncPositionY, syncPositionZ);
        }
        
        var rotation = transform.eulerAngles;
        if (VectorsChanged(rotation, _lastValidRotation, RotationMask, rotationThreshold))
        {
            _lastValidRotation = rotation;
            SyncRotation(rotation, syncRotationX, syncRotationY, syncRotationZ);
        }
        
        var scale = transform.localScale;
        if (VectorsChanged(scale, _lastValidScale, ScaleMask, scaleThreshold))
        {
            _lastValidScale = scale;
            SyncScale(scale, syncScaleX, syncScaleY, syncScaleZ);
        }
    }

    [RepoSteamRPC(RPCTarget.Clients)]
    public void SyncPositionRPC(Vector3 position, bool syncX, bool syncY, bool syncZ)
    {
        var posToSet = transform.position;
        
        if (syncX)
            posToSet.x = position.x;
        if (syncY)
            posToSet.y = position.y;
        if (syncZ)
            posToSet.z = position.z;
        
        transform.position = posToSet;
    }

    [RepoSteamRPC(RPCTarget.Clients)]
    public void SyncRotationRPC(Vector3 rotation, bool syncX, bool syncY, bool syncZ)
    {
        var rotToSet = transform.eulerAngles;

        if (syncX)
            rotToSet.x = rotation.x;
        if (syncY)
            rotToSet.y = rotation.y;
        if (syncZ)
            rotToSet.z = rotation.z;
        
        transform.eulerAngles = rotToSet;
    }

    [RepoSteamRPC(RPCTarget.Clients)]
    public void SyncScaleRPC(Vector3 scale, bool syncX, bool syncY, bool syncZ)
    {
        var scaleToSet = transform.localScale;
        
        if (syncX)
            scaleToSet.x = scale.x;
        if (syncY)
            scaleToSet.y = scale.y;
        if (syncZ)
            scaleToSet.z = scale.z;
        
        transform.localScale = scaleToSet;
    }

    private static Vector3 MaskVector(Vector3 vector, Vector3 mask) =>
        new(vector.x * mask.x, vector.y * mask.y, vector.z * mask.z);

    private static bool VectorsChanged(Vector3 left, Vector3 right, Vector3 mask, float threshold) =>
        Vector3.Distance(MaskVector(left, mask), MaskVector(right, mask)) > threshold;
}