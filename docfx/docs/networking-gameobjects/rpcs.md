# RPCs

> [!WARNING]
> The current implementation of RPC's is fairly basic and isn't well optimized.
> I don't recommend calling RPC's every frame.
> If you need to have a variable or value networked, consider using [NetworkedProperties](./networked-properties.md) instead.

RPCs are supported and are simple to declare and use.

## Quick Start

Let's start with a MonoBehaviour we want to add an RPC to.

```csharp
public class ExampleBehaviour : MonoBehaviour 
{
    
}
```

We must first make our `ExampleBehaviour` class `partial`.

[!code-csharp[](../../examples/rpcs/ExampleBehaviour.cs?highlight=1#L1-L4)]

Now we can add an RPC to our `ExampleBehaviour`.

[!code-csharp[](../../examples/rpcs/ExampleBehaviour.cs?highlight=5-8#L5-L13)]

> [!WARNING]
> RPC Methods must end with `RPC`.
> 
> :x: Invalid
> ```csharp
> public void RpcExample(string message, Vector3 position)
> ```
> :x: Invalid
> ```csharp
> public void Example(string message, Vector3 position)
> ```
> :heavy_check_mark: Valid
> [!code-csharp[](../../examples/rpcs/ExampleBehaviour.cs#L9)]

To get the Source Generators to generate the networking code for this RPC, we need to add the [RepoSteamRPC](../../api/RepoSteamNetworking.API.Unity.RepoSteamRPCAttribute.yml) Attribute to the RPC method we declared.

[!code-csharp[](../../examples/rpcs/ExampleBehaviour.cs?highlight=2,6#L14-L24)]

By default, an RPC will run on both the clients and the host, for this example we'd like to only run the RPC on the clients, so lets change the target to be `RPCTarget.Clients`.

[!code-csharp[](../../examples/rpcs/ExampleBehaviour.cs?highlight=2,7#L25-L36)]

Now the Source Generator should have generated a new method called `Example` in an auto-generated file. We can now call this method in our `ExampleBehaviour`, for now we'll do so in `Start`.

[!code-csharp[](../../examples/rpcs/ExampleBehaviour.cs?highlight=15#L37-L53)]

The auto-generated method `Example` handles all the work for networking our RPC `ExampleRPC` to the right targets. This means that `ExampleRPC` will run on all clients when we call the method `Example`.

## RPC Parameter Types
RPC parameters are serialized by Odin Serializer, so anything supported by Odin Serializer is supported in RPC's

## RPC Overloading
The Source Generators support handling multiple RPC methods with the same name or method overloads. 