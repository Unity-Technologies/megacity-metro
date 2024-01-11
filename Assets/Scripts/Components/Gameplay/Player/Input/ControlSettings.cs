using Unity.Entities;
using Unity.Mathematics;

namespace Unity.MegacityMetro.Gameplay
{
    /// <summary>
    /// Control settings values
    /// </summary>
    public struct ControlSettings : IComponentData
    {
        public float MouseSensitivity;
        public float AimAssistanceSensitivity;
        public float2 AccelerationRange;
        public bool InverseLookHorizontal;
        public bool InverseLookVertical;
    }
}