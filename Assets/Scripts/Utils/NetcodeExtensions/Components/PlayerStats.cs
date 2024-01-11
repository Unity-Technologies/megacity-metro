using Unity.Collections;
using Unity.Entities;

namespace Unity.NetCode.Extensions
{
    public struct PlayerStats : IComponentData
    {
        public int NetworkId;
        public FixedString64Bytes Name;
        public float FPS;
        public uint NumberOfSystems;

        public int EstimatedRTT;
        public int DeviationRTT;
        public bool ShouldUpdate;
    }

    public struct EntityCountStats : IComponentData
    {
    }
}
