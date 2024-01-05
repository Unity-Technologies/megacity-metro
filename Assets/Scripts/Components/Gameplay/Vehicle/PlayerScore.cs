using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

namespace Unity.MegacityMetro.Gameplay
{
    /// <summary>
    /// Player score
    /// </summary>
    public struct PlayerScore : IComponentData
    {
        [GhostField(Quantization = 1000)] public float Value;
        public bool IsLocalPlayer;
        [GhostField] public int Kills;
        [GhostField] public FixedString64Bytes KilledPlayer;
        [GhostField] public FixedString64Bytes KillerName;
    }
}