# Network Packets

RepoSteamNetworking allows for sending and receiving arbitrary data as [Network Packets](../api/RepoSteamNetworking.API.NetworkPacket-1.yml). Network Packets can contain any amount of data that can be serialized/deserialized by Odin Serializer.

You can have as many custom packets as you want.

## Creating Packets

Create a new class that inherits from [NetworkPacket](../api/RepoSteamNetworking.API.NetworkPacket-1.yml).

Here's an example of what a Network Packet could look like.
```csharp
using RepoSteamNetworking.API;
using RepoSteamNetworking.Networking.Serialization;

namespace MyModNamespace;

public class MyCustomPacket : NetworkPacket<MyCustomPacket> 
{
    // Define properties to access here.
    public string MyString { get; set; }
    public int CoolNumber { get; set; }
    
    // Write data to the buffer.
    protected override void WriteData(SocketMessage socketMessage)
    {
        socketMessage.Write(MyString);
        socketMessage.Write(CoolNumber);
    }

    // Read data from the buffer.
    protected override void ReadData(SocketMessage socketMessage)
    {
        // Type must be defined when reading from the buffer.
        MyString = socketMessage.Read<string>();
        CoolNumber = socketMessage.Read<int>();
    }
}
```
The order that you read and write data in is crucial.

### Serialization/Deserialization Support

Not only can we serialize/deserialize primitives, but we can serialize classes and structs too.
For example, we have a custom struct that contains some data.
```csharp
using System;

// Mark it as serializable.
[Serializable]
public struct UserMessages 
{
    public Dictionary<string, string[]> UserMessageDict;
}
```

We can serialize/deserialize it easily thanks to Odin Serializer
```csharp
using RepoSteamNetworking.API;
using RepoSteamNetworking.Networking.Serialization;

namespace MyModNamespace;

public class UserMessagesPacket : NetworkPacket<UserMessagesPacket> 
{
    public UserMessages Data { get; set; }
    
    protected override void WriteData(SocketMessage socketMessage)
    {
        socketMessage.Write(Data);
    }

    protected override void ReadData(SocketMessage socketMessage)
    {
        Data = socketMessage.Read<UserMessages>();
    }
}
```

## Registering Packets

We'll use the first example as our custom packet for this section.

Before we can send and receive our packet, we need to register it.

Packets can be registered using [RepoSteamNetwork.RegisterPacket](../api/RepoSteamNetworking.API.RepoSteamNetwork.yml#RepoSteamNetworking_API_RepoSteamNetwork_RegisterPacket__1).
So using our packet `MyCustomPacket`, we would register it like so:

```csharp
using RepoSteamNetworking.API;

// This method can be called whatever you want, it doesn't even need to be a method. this is just for our example.
// Just make sure to call the method somewhere if you do it this way.
public static void RegisterAllPackets()
{
    // The only line you actually need.
    RepoSteamNetwork.RegisterPacket<MyCustomPacket>();
}
```

Now we can send packets, but we still need to be able to receive them. To do that, we need to add a callback for our packet `MyCustomPacket`.

## Packet Callbacks

Callbacks are needed to handle receiving custom packets. Callbacks can be added to a packet using [RepoSteamNetwork.AddCallback](../api/RepoSteamNetworking.API.RepoSteamNetwork.yml#RepoSteamNetworking_API_RepoSteamNetwork_AddCallback__1_System_Action___0__).
Keeping `MyCustomPacket` as our packet of choice we can add a callback like so:

```csharp
using RepoSteamNetworking.API;

// This method can be called whatever you want, it doesn't even need to be a method. this is just for our example.
// Just make sure to call the method somewhere if you do it this way.
public static void RegisterPacketCallbacks() 
{
    // The only line you actually need.
    RepoSteamNetwork.AddCallback<MyCustomPacket>(OnMyCustomPacketReceived);
}

// Called when we receive the packet `MyCustomPacket` locally.
internal static void OnMyCustomPacketReceived(MyCustomPacket packet)
{
    // Do something with the data in the packet.
    UnityEngine.Debug.Log(packet.MyString);
}
```

## Sending Packets

Packets can be sent using [RepoSteamNetwork.SendPacket](../api/RepoSteamNetworking.API.RepoSteamNetwork.yml#RepoSteamNetworking_API_RepoSteamNetwork_SendPacket__1___0_RepoSteamNetworking_Networking_NetworkDestination_).

Now we can send our packet properly:
```csharp
var myPacket = new MyCustomPacket 
{
    MyString = "Hello this is my custom packet string",
    CoolNumber = 420,
};

// Send this packet to everybody.
RepoSteamNetwork.SendPacket(myPacket);
```
### Sending Packets to specific destinations.
By default, [SendPacket](../api/RepoSteamNetworking.API.RepoSteamNetwork.yml#RepoSteamNetworking_API_RepoSteamNetwork_SendPacket__1___0_RepoSteamNetworking_Networking_NetworkDestination_) sends the packet to every client, including the client that is sending the packet.
This behavior can be altered by changing the `destination` parameter of the method. See the [NetworkDestination](../api/RepoSteamNetworking.Networking.NetworkDestination.yml) Enum for a list of accepted values.

You can also send a packet to a specific client by setting the [Target](../api/RepoSteamNetworking.Networking.Packets.PacketHeader.yml#RepoSteamNetworking_Networking_Packets_PacketHeader_Target) of the packets [Header](../api/RepoSteamNetworking.API.NetworkPacket.yml#RepoSteamNetworking_API_NetworkPacket_Header) to the `SteamId` of the client you want to send to,
and then sending the packet `RepoSteamNetwork.SendPacket(myPacket, NetworkDestination.PacketTarget)`.

An alternative way to send a packet to a specific user is to use one of the [extension methods](../api/RepoSteamNetworking.API.NetworkExtensions.yml):
```csharp
var targetSteamId = ...;

targetSteamId.SendPacket(myPacket);

var targetPlayerAvatar = ...;

targetPlayerAvatar.SendPacket(myPacket);
```