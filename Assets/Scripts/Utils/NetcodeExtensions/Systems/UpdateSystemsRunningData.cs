using Unity.Burst;
using Unity.Entities;

namespace Unity.NetCode.Extensions
{
    [UpdateAfter(typeof(UpdateFPSData))]
    [UpdateInGroup(typeof(NetCodePanelStats))]
    public partial struct UpdateSystemsRunningData : ISystem
    {
        private uint m_PreviousFrameVersion;
        private uint m_CurrentFrameVersion;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerStats>();
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var playerStats = SystemAPI.GetSingletonRW<PlayerStats>().ValueRW;
            m_CurrentFrameVersion = state.EntityManager.GlobalSystemVersion - m_PreviousFrameVersion;
            m_PreviousFrameVersion = state.EntityManager.GlobalSystemVersion;
            playerStats.NumberOfSystems = m_CurrentFrameVersion;
            SystemAPI.SetSingleton(playerStats);
        }
    }
}