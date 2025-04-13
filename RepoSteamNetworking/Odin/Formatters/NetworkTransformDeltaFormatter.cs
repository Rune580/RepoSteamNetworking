using RepoSteamNetworking.Networking.Data;
using RepoSteamNetworking.Utils;
using Sirenix.Serialization;

namespace RepoSteamNetworking.Odin.Formatters;

public class NetworkTransformDeltaFormatter : MinimalBaseFormatter<NetworkTransformDelta>
{
    private static readonly Serializer<BitFlags> BitFlagsSerializer = Serializer.Get<BitFlags>();
    private static readonly Serializer<float> FloatSerializer = Serializer.Get<float>();

    public override void Read(ref NetworkTransformDelta value, IDataReader reader)
    {
        var flags = BitFlagsSerializer.ReadValue(reader);

        if (flags[0])
            value.PositionX = FloatSerializer.ReadValue(reader);
        if (flags[1])
            value.PositionY = FloatSerializer.ReadValue(reader);
        if (flags[2])
            value.PositionZ = FloatSerializer.ReadValue(reader);
        
        if (flags[3])
            value.RotationX = FloatSerializer.ReadValue(reader);
        if (flags[4])
            value.RotationY = FloatSerializer.ReadValue(reader);
        if (flags[5])
            value.RotationZ = FloatSerializer.ReadValue(reader);
        
        if (flags[6])
            value.ScaleX = FloatSerializer.ReadValue(reader);
        if (flags[7])
            value.ScaleY = FloatSerializer.ReadValue(reader);
        if (flags[8])
            value.ScaleZ = FloatSerializer.ReadValue(reader);
    }

    public override void Write(ref NetworkTransformDelta value, IDataWriter writer)
    {
        var flags = new BitFlags
        {
            [0] = value.PositionX.HasValue,
            [1] = value.PositionY.HasValue,
            [2] = value.PositionZ.HasValue,
            [3] = value.RotationX.HasValue,
            [4] = value.RotationY.HasValue,
            [5] = value.RotationZ.HasValue,
            [6] = value.ScaleX.HasValue,
            [7] = value.ScaleY.HasValue,
            [8] = value.ScaleZ.HasValue
        };
        
        BitFlagsSerializer.WriteValue(flags, writer);

        if (value.PositionX.HasValue)
            FloatSerializer.WriteValue(value.PositionX.Value, writer);
        if (value.PositionY.HasValue)
            FloatSerializer.WriteValue(value.PositionY.Value, writer);
        if (value.PositionZ.HasValue)
            FloatSerializer.WriteValue(value.PositionZ.Value, writer);
        
        if (value.RotationX.HasValue)
            FloatSerializer.WriteValue(value.RotationX.Value, writer);
        if (value.RotationY.HasValue)
            FloatSerializer.WriteValue(value.RotationY.Value, writer);
        if (value.RotationZ.HasValue)
            FloatSerializer.WriteValue(value.RotationZ.Value, writer);
        
        if (value.ScaleX.HasValue)
            FloatSerializer.WriteValue(value.ScaleX.Value, writer);
        if (value.ScaleY.HasValue)
            FloatSerializer.WriteValue(value.ScaleY.Value, writer);
        if (value.ScaleZ.HasValue)
            FloatSerializer.WriteValue(value.ScaleZ.Value, writer);
    }
}