using Unity.Entities;
using Unity.Mathematics;

namespace Unity.MegacityMetro.Gameplay
{
    /// <summary>
    /// Spawn point for players
    /// </summary>
    public struct SpawnPointElement : IBufferElementData
    {
        public float3 Value;
    }
}
