using Unity.Entities;

namespace Unity.MegacityMetro.Gameplay
{
    public struct PlayerLocationBounds : IComponentData
    {
        public bool IsInside;
    }
}