API for R.E.P.O that allows for networking using Steamworks.

Heavily W.I.P. proper documentation along with nuget package will come closer to a more complete release.

basic documentation is as follows

### Set version compatibility

#### Method A

```csharp
[RSNVersionCompatibility(VersionCompatibility.Strict, optional: false)] // Defaults
[BepInPlugin(...)]
class ExampleModPlugin : BaseUnityPlugin {...}
```

#### Method B

```csharp
class ExampleModPlugin : BaseUnityPlugin 
{
    private void Awake() 
    {
        RepoSteamNetwork.SetVersionCompatibility(VersionCompatibility.Strict, plugin: this);
    }
}
```

### Create network packet
```csharp
public class ExamplePacket : NetworkPacket<ExamplePacket>
{
    public string ExampleDataString;
    public Vector3 ExampleVector;
    
    protected override void WriteData(SocketMessage socketMessage) 
    {
        socketMessage.Write(ExampleDataString);
        socketMessage.Write(ExampleVector);
    }
    
    protected override void ReadData(SocketMessage socketMessage) 
    {
        ExampleDataString = socketMessage.Read<string>();
        ExampleVector = socketMessage.Read<Vector3>();
    }
}
```

### Register network packet
```csharp
RepoSteamNetwork.RegisterPacket<ExamplePacket>();
```

### Setup callback

#### Method A
Register callback
```csharp
{
    ...
    RepoSteamNetwork.AddCallback<ExamplePacket>(OnExamplePacketReceived);
    ...
}

private static void OnExamplePacketReceived(ExamplePacket packet) 
{
    // Do stuff with your packet data
    
    Debug.Log($"Received ExamplePacket Data: ({packet.ExampleDataString}, {packet.ExampleVector})");
}
```
#### Method B
Do your callback in `ReadData` in your packet
```csharp
public class ExamplePacket : NetworkPacket<ExamplePacket>
{
    ...
    
    protected override void ReadData(SocketMessage socketMessage) 
    {
        ExampleDataString = socketMessage.Read<string>();
        ExampleVector = socketMessage.Read<Vector3>();
        
        // Do stuff with your data here
        
        Debug.Log($"Received ExamplePacket Data: ({ExampleDataString}, {ExampleVector})");
    }
}
```

### Send Packet
```csharp
var packet = new ExamplePacket 
{
    ExampleDataString = "Example string here",
    ExampleVector = Vector3.One,
}

// By default sends packet to everyone including the user who sends the packet
RepoSteamNetwork.SendPacket(packet);
// or send it only to the host
RepoSteamNetwork.SendPacket(packet, NetworkDestination.HostOnly);
```