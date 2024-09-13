using Unity.Entities;
using Unity.Mathematics;

namespace Unity.MegacityMetro.Gameplay
{
    public struct BlimpComponent : IComponentData
    {
        public float3 StartingPosition;
        public float TravelRadius;
        public float RandomStartPoint;
        public float LookAheadValue;
        public float Rotation;
        public float RotationSpeed;
    }
}