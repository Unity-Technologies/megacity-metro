using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.NetCode.Extensions;

namespace Unity.MegacityMetro.Gameplay
{
    /// <summary>
    /// Player score Aspect
    /// </summary>
    public readonly partial struct PlayerScoreAspect : IAspect
    {
        public readonly Entity Self;
        readonly RefRW<PlayerScore> m_Value;
        readonly RefRO<PlayerPlatform> m_Platform;
        readonly RefRO<PlayerName> m_Name;
        readonly RefRO<GhostOwner> m_GhostOwnerComponent;
        

        public int Platform => m_Platform.ValueRO.Value;
        public int Kills => m_Value.ValueRW.Kills;
        public FixedString64Bytes Name => m_Name.ValueRO.Name;
        public int Value => (int)math.trunc(m_Value.ValueRO.Value / 10f);
        public int NetworkId => m_GhostOwnerComponent.ValueRO.NetworkId;
        public FixedString64Bytes Killed => m_Value.ValueRO.KilledPlayer;
        public bool IsOwnerPlayer => m_Value.ValueRO.IsLocalPlayer;
        public void SetIsLocalPlayer(ref Entity localPlayerRefEntity)
        {
            m_Value.ValueRW.IsLocalPlayer = Self.Equals(localPlayerRefEntity);
        }
    }
}
