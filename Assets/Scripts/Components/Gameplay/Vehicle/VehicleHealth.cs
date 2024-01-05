using Unity.Entities;
using Unity.NetCode;

namespace Unity.MegacityMetro.Gameplay
{
    /// <summary>
    /// Vehicle health
    /// </summary>
    public struct VehicleHealth : IComponentData
    {
        [GhostField(Quantization = 1000)] public float Value;
        [GhostField] public byte IsDead;
        [GhostField] public float LookAtEnemyDegrees;
        public float _deathTimer;
        public byte _DeathFXState;
    }
}
