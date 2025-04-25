# Networked Properties
Networked Properties are `fields` or `properties` that are automatically networked.

Networked Properties, much like RPCs, can only be declared in a class that is marked `partial`.

## Declaring a Networked Property

### Fields

Fields can be networked using the [NetworkedProperty](../../api/RepoSteamNetworking.API.Unity.NetworkedPropertyAttribute.yml) Attribute.
Given a field:
```csharp
public int testNumber;
```

Add the attribute to the field:
```csharp
using UnityEngine;
using RepoSteamNetworking.API.Unity;

public partial class ExampleBehaviour : MonoBehaviour  
{
    [NetworkedProperty]
    public int testNumber;
}
```

A new `property` called `TestNumber` will be generated in an auto-generated file. Getting and setting to and from this generated property is fully networked.

The name will by default be a [PascalCase](https://en.wikipedia.org/wiki/PascalCase) name of the field. This behavior can be overriden by setting the [OverridePropertyName](../../api/RepoSteamNetworking.API.Unity.NetworkedPropertyAttribute.yml#RepoSteamNetworking_API_Unity_NetworkedPropertyAttribute_OverridePropertyName) named parameter.
You **must** use the generated property to have the value be networked!

### Properties

Properties can also be networked using the [NetworkedProperty](../../api/RepoSteamNetworking.API.Unity.NetworkedPropertyAttribute.yml) Attribute.
However, the property must be `partial`:
```csharp
public partial int TestNumber { get; set; }
```

Add the attribute to the property:
```csharp
using UnityEngine;
using RepoSteamNetworking.API.Unity;

public partial class ExampleBehaviour : MonoBehaviour  
{
    [NetworkedProperty]
    public partial int TestNumber { get; set; }
}
```

Unlike with a `field`, you can get and set the value from the declared property itself. The property *does* generate a backing `field` called `_testNumberBackingField`.
The name will by default be a `_lowerCamelCase` name of the property. This behavior can be overriden by setting the [OverrideBackingField](../../api/RepoSteamNetworking.API.Unity.NetworkedPropertyAttribute.yml#RepoSteamNetworking_API_Unity_NetworkedPropertyAttribute_OverrideBackingField) named parameter.
The backing field can be set directly if you need to set the value without networking it.

## Write Permissions
By default, both the host and clients can set Networked Properties. Set the [WritePermissions](../../api/RepoSteamNetworking.API.Unity.NetworkedPropertyAttribute.yml#RepoSteamNetworking_API_Unity_NetworkedPropertyAttribute_WritePermissions) named parameter to change who is allowed to set the value.
```csharp
using UnityEngine;
using RepoSteamNetworking.API;

public partial class ExampleBehaviour : MonoBehaviour  
{
    [NetworkedProperty(WritePermissions = NetworkPermission.Host)] // Only allow the host to change this value.
    public partial int TestNumber { get; set; }
}
```

## Callbacks
You can set a method to be called when the Networked Property gets updated:
```csharp
using UnityEngine;
using RepoSteamNetworking.API;

public partial class ExampleBehaviour : MonoBehaviour  
{
    [NetworkedProperty(CallbackMethodName = nameof(OnTestNumberChanged))]
    public partial int TestNumber { get; set; }
    
    private void OnTestNumberChanged(int oldValue, int newValue)
    {
        // Do something with the values.
    }
}
```