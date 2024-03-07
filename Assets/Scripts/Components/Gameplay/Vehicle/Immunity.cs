using Unity.Entities;
using Unity.NetCode;

namespace Unity.MegacityMetro.Gameplay
{
    /// <summary>
    /// Immunity settings
    /// </summary>
    public struct Immunity : IComponentData
    {
        public float Duration;
        [GhostField] public float Counter;
        
        public byte _immunityFXState;
    }
}