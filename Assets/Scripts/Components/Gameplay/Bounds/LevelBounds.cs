using Unity.Entities;
using Unity.Mathematics;

namespace Unity.MegacityMetro.Gameplay
{
    public struct LevelBounds : IComponentData
    {
        public float MaxDistance;
    }

    public struct LevelBoundsContainer : IComponentData
    {
        public Entity BoundsPrefab;
    }
}
