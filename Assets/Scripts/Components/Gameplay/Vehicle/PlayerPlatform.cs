using Unity.Entities;
using Unity.NetCode;

namespace Unity.MegacityMetro.Gameplay
{
    public struct PlayerPlatform : IComponentData 
    {
        [GhostField] public int Value;
    }
}