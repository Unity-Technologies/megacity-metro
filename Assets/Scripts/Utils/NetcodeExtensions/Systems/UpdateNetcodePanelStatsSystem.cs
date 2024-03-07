using UnityEngine;
using Unity.Entities;

namespace Unity.NetCode.Extensions
{
    /// <summary>
    /// Collects system information metrics
    /// and send all the data to the Info panel UI
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.LocalSimulation | WorldSystemFilterFlags.ClientSimulation, WorldSystemFilterFlags.LocalSimulation | WorldSystemFilterFlags.ClientSimulation)]
    [UpdateAfter(typeof(NetCodePanelStats))]
    public partial class NetCodePanelStatsPresentation : ComponentSystemGroup
    {
    }

    [UpdateInGroup(typeof(NetCodePanelStatsPresentation))]
    public partial struct UpdateNetcodePanelStatsSystem : ISystem, ISystemStartStop
    {
        private EntityQuery m_TotalPlayersQuery;
        private EntityQuery m_TotalEntities;

        public void OnCreate(ref SystemState state)
        {
#if !(DEVELOPMENT_BUILD || UNITY_EDITOR)
            state.Enabled = false;
#endif

            m_TotalPlayersQuery = state.GetEntityQuery(ComponentType.ReadOnly<GhostOwner>());
            m_TotalEntities = state.GetEntityQuery(ComponentType.Exclude<EntityCountStats>());
            state.RequireForUpdate<GhostOwnerIsLocal>();
            state.RequireForUpdate<GhostCount>();
            state.RequireForUpdate<PlayerStats>();
        }
        
        public void OnStartRunning(ref SystemState state)
        {
            if (NetcodePanelStats.Instance == null)
                return;

            NetcodePanelStats.Instance.Enable();
        }

        public void OnStopRunning(ref SystemState state)
        {
            if (NetcodePanelStats.Instance == null)
                return;

            NetcodePanelStats.Instance.Disable();
        }
        
        public void OnUpdate(ref SystemState state)
        {
            if (NetcodePanelStats.Instance == null)
                return;
            
            var playerStats = SystemAPI.GetSingletonRW<PlayerStats>().ValueRO;
            if (!playerStats.ShouldUpdate)
                return;
            
            var quality = QualitySettings.names[QualitySettings.GetQualityLevel()];
            var ghostCount = SystemAPI.GetSingleton<GhostCount>();
            var entityCount = m_TotalEntities.CalculateEntityCount(); 
            
            NetcodePanelStats.Instance.SetFPSLabel(playerStats.FPS);
            NetcodePanelStats.Instance.SetNumberOfPlayers(m_TotalPlayersQuery.CalculateEntityCount());
            NetcodePanelStats.Instance.SetPingLabel(playerStats.EstimatedRTT, playerStats.DeviationRTT);
            NetcodePanelStats.Instance.SetSystemsLabel(playerStats.NumberOfSystems);
            NetcodePanelStats.Instance.SetEntitiesLabel(entityCount);
            NetcodePanelStats.Instance.UpdateQualityLabel(quality);
            NetcodePanelStats.Instance.SetGhostsLabel(ghostCount);
        }
    }
}
