using Unity.Burst;
using Unity.Entities;

namespace Unity.NetCode.Extensions
{
    [UpdateAfter(typeof(UpdateFPSData))]
    [UpdateInGroup(typeof(NetCodePanelStats))]
    public partial struct UpdatePingData : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerStats>();
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var playerStats = SystemAPI.GetSingletonRW<PlayerStats>().ValueRW;
            if (!playerStats.ShouldUpdate)
                return;

            foreach (var (networkId, networkSnapshotAck) in SystemAPI.Query<RefRO<NetworkId>, RefRO<NetworkSnapshotAck>>())
            {
                if (playerStats.NetworkId == networkId.ValueRO.Value)
                {
                    playerStats.EstimatedRTT = (int)networkSnapshotAck.ValueRO.EstimatedRTT;
                    playerStats.DeviationRTT = (int)networkSnapshotAck.ValueRO.DeviationRTT;
                }
            }
            SystemAPI.SetSingleton(playerStats);
        }
    }
}