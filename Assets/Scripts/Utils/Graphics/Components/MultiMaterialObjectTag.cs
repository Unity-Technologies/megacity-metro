using Unity.Entities;
using Unity.NetCode;

namespace Unity.MegacityMetro.Utils
{
    public struct MultiMaterialObjectTag : IComponentData
    {
    }

    public struct MultiMaterialUpdated : IComponentData
    {
    }
    
    [GhostComponent(PrefabType = GhostPrefabType.Client)]
    public struct SetPlayerColorTag : IComponentData
    {
    }
}
