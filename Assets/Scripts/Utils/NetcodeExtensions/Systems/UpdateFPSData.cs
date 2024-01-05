using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Unity.NetCode.Extensions
{
    [UpdateAfter(typeof(InitializeNetcodePanelStatsSystem))]
    [UpdateInGroup(typeof(NetCodePanelStats))]
    public partial struct UpdateFPSData : ISystem
    {
        private float m_UpdateRateSeconds;
        private int m_FarmeCount;
        private float m_DeltaTime;
        private float m_FPS;
        
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerStats>();
            m_UpdateRateSeconds = 4.0F;
            m_FarmeCount = 0;
            m_DeltaTime = 0.0F;
            m_FPS = 0.0F;
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var playerStats = SystemAPI.GetSingletonRW<PlayerStats>().ValueRW;
            playerStats.ShouldUpdate = false;
            m_DeltaTime += SystemAPI.Time.DeltaTime;
            m_FarmeCount++;
            if (m_DeltaTime > 1.0 / m_UpdateRateSeconds)
            {
                m_FPS = m_FarmeCount / m_DeltaTime;
                m_FarmeCount = 0;
                m_DeltaTime -= 1.0F / m_UpdateRateSeconds;
                playerStats.FPS = (int)math.trunc(m_FPS);
                playerStats.ShouldUpdate = true;
                
            }
            SystemAPI.SetSingleton(playerStats);
        }
    }
}