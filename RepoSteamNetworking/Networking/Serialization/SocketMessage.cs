using System;
using System.IO;
using RepoSteamNetworking.Networking.Packets;
using Sirenix.Serialization;

namespace RepoSteamNetworking.Networking.Serialization;

public class SocketMessage : IDisposable
{
    private readonly MemoryStream _buffer;
    private IDataWriter? _writer;
    private IDataReader? _reader;
    
    private IDataWriter Writer
    {
        get
        {
            _writer ??= SerializationUtility.CreateWriter(_buffer, new SerializationContext(), DataFormat.Binary);
            return _writer;
        }
    }

    private IDataReader Reader
    {
        get
        {
            _reader ??= SerializationUtility.CreateReader(_buffer, new DeserializationContext(), DataFormat.Binary);
            return _reader;
        }
    }
    
    public SocketMessage()
    {
        _buffer = new MemoryStream();
    }

    public SocketMessage(byte[] buffer)
    {
        _buffer = new MemoryStream(buffer);
    }

    internal byte[] GetBytes() => _buffer.ToArray();

    /// <summary>
    /// Writes a value of type <typeparamref name="T"/> to the internal buffer of the socket message.
    /// </summary>
    /// <typeparam name="T">The type of the value to be written.</typeparam>
    /// <param name="value">The value to be written to the buffer.</param>
    /// <remarks>
    /// Uses Odin Serializer to write the value to the buffer.
    /// </remarks>
    public SocketMessage Write<T>(T value)
    {
        SerializationUtility.SerializeValue(value, Writer);
        return this;
    }

    public SocketMessage WritePacketHeader(PacketHeader header)
    {
        Write(header.PacketId);
        Write((byte)header.Destination);
        Write(header.Sender.Value);
        Write(header.Target.Value);
        return this;
    }

    /// <summary>
    /// Reads a value of type <typeparamref name="T"/> from the internal buffer of the socket message.
    /// </summary>
    /// <typeparam name="T">The type of the value to be read from the buffer.</typeparam>
    /// <returns>The deserialized value of type <typeparamref name="T"/>.</returns>
    /// <remarks>
    /// Uses Odin Serializer to read the value from the buffer.
    /// </remarks>
    public T Read<T>()
    {
        return SerializationUtility.DeserializeValue<T>(Reader);
    }

    public PacketHeader ReadPacketHeader()
    {
        var packetId = Read<int>();
        var destination = (NetworkDestination)Read<byte>();
        var sender = Read<ulong>();
        var target = Read<ulong>();

        return new PacketHeader
        {
            PacketId = packetId,
            Destination = destination,
            Sender = sender,
            Target = target
        };
    }
    
    public void Dispose()
    {
        _buffer.Dispose();
        _writer?.Dispose();
        _reader?.Dispose();
    }
}