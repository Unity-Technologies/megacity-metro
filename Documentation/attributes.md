# Documentation Contents and Quick Links

- [Baking Version](#baking-version)
- [Prefab Type](#prefab-type)
- [Ghost Field](#ghost-field)
- [World System Filter](#world-system-filter)

## Baking Version

The `BakingVersion` attribute allows Unity to compile the Authoring conversion code once by adding `[BakingVersion("megacity-metro", 1)]` to the baking classes. The first parameter indicates the username, while the second parameter denotes the current version. If the Baker lacks this attribute, it will be compiled every time the domain reloads. [Read more...](https://docs.unity3d.com/Packages/com.unity.entities@1.2/api/Unity.Entities.BakingVersionAttribute.html)

## Prefab Type

The `PrefabType` defines the type of prefab to be assigned. By including `[GhostComponent(PrefabType = GhostPrefabType.PredictedClient)]`, the component updates properties according to the assigned type. [Read more...](https://docs.unity3d.com/Packages/com.unity.netcode@1.2/api/Unity.NetCode.GhostPrefabType.html)

## Ghost Field

The `GhostField` enables server and client synchronization for a specific parameter. This attribute should be added to a component belonging to a Ghost Prefab.
**Note:** All ghosts must be prefabs instantiated by the server with the Ghost component attached. [Read more...](https://docs.unity3d.com/Packages/com.unity.netcode@1.2/api/Unity.NetCode.GhostFieldAttribute.html)

## World System Filter

The `WorldSystemFilter` attribute is designed to specify the system's target world. The Netcode package creates an independent world for the server and client. This attribute filters which `Client Target` executes the system. [Read more...](https://docs.unity3d.com/Packages/com.unity.entities@1.2/api/Unity.Entities.WorldSystemFilterFlags.html)
