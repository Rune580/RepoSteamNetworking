namespace RepoSteamNetworking.Networking.Data;

internal readonly record struct PaletteSubId
{
    private const ulong SubIdMask = ulong.MaxValue >>> 32;
    
    private readonly ulong _value;
    
    public uint GuidPaletteId => (uint)(_value >>> 32);
    public uint SubId => (uint)(_value & SubIdMask);

    public PaletteSubId(uint paletteId, uint subId)
    {
        _value = (ulong)paletteId << 32 | subId;
    }

    public PaletteSubId(ulong packedPropId)
    {
        _value = packedPropId;
    }

    public static implicit operator ulong(PaletteSubId paletteSubId) => paletteSubId._value;
    
    public static implicit operator PaletteSubId(ulong paletteSubId) => new(paletteSubId);
}