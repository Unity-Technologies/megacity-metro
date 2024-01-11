using Unity.Burst;
using Unity.Entities;

namespace Unity.NetCode.Extensions
{
    [WorldSystemFilter(WorldSystemFilterFlags.LocalSimulation | WorldSystemFilterFlags.ClientSimulation, WorldSystemFilterFlags.LocalSimulation | WorldSystemFilterFlags.ClientSimulation)]
    public partial class NetCodePanelStats : ComponentSystemGroup
    {
    }

    [UpdateInGroup(typeof(NetCodePanelStats))]
    public partial struct InitializeNetcodePanelStatsSystem : ISystem
    {
        private EntityQuery m_LocalPlayerQuery;
        public void OnCreate(ref SystemState state)
        {
#if !(DEVELOPMENT_BUILD || UNITY_EDITOR)
            state.Enabled = false;
#endif
            
            m_LocalPlayerQuery = state.GetEntityQuery(ComponentType.ReadOnly<GhostOwner>(),
                                                      ComponentType.ReadOnly<GhostOwnerIsLocal>(),
                                                      ComponentType.ReadOnly<PlayerName>(),
                                                      ComponentType.Exclude<PlayerStats>());
            state.RequireForUpdate(m_LocalPlayerQuery);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.EntityManager.AddComponent<PlayerStats>(m_LocalPlayerQuery);
            foreach (var (playerName, ghostOwner, entity) in 
                            SystemAPI.Query<RefRO<PlayerName>,RefRO<GhostOwner>>()
                            .WithAll<PlayerStats>()
                            .WithEntityAccess())
            {
                var stats = new PlayerStats
                {
                    NetworkId = ghostOwner.ValueRO.NetworkId,
                    Name = playerName.ValueRO.Name,
                };
                state.EntityManager.SetComponentData(entity, stats);
            }
        }
    }
}
