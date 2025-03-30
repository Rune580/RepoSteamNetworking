namespace RepoSteamNetworking.Networking.NetworkedProperties;

internal readonly record struct BehaviourPropertyId
{
    private const ulong PropIdMask = ulong.MaxValue >>> 32;
    
    private readonly ulong _value;
    
    public uint BehaviourClassId => (uint)(_value >>> 32);
    public uint PropertyId => (uint)(_value & PropIdMask);

    public BehaviourPropertyId(uint classId, uint propId)
    {
        _value = (ulong)classId << 32 | propId;
    }

    public BehaviourPropertyId(ulong packedPropId)
    {
        _value = packedPropId;
    }

    public static implicit operator ulong(BehaviourPropertyId behaviourPropId) => behaviourPropId._value;
    
    public static implicit operator BehaviourPropertyId(ulong packedPropId) => new(packedPropId);
}