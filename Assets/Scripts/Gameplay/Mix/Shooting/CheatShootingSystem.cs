using Unity.Entities;
using Unity.NetCode;
using static Unity.Entities.SystemAPI;
using Unity.NetCode.Extensions;

namespace Unity.MegacityMetro.Gameplay
{
#if DEVELOPMENT_BUILD || UNITY_EDITOR
    /// <summary>
    /// System to cheat shooting.
    /// </summary>
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    public partial struct CheatShootingSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkTime>();
#if !UNITY_EDITOR            
            state.Enabled = false;
#endif
        }

        public void OnUpdate(ref SystemState state)
        {
            var netTime = GetSingleton<NetworkTime>();
            if (!netTime.IsFirstTimeFullyPredictingTick)
                return;
            
            bool isServer = state.World.IsServer();

            foreach (var (ownerScore, input, playerName, health) in
                         Query<RefRW<PlayerScore>,
                         RefRO<PlayerVehicleInput>,
                         RefRO<PlayerName>,
                         RefRW<VehicleHealth>>().WithAll<Simulate>())
            {
                if (isServer && input.ValueRO.Cheat_1 && health.ValueRO.IsDead == 0)
                {
                    health.ValueRW.Value -= 1f;
                    ownerScore.ValueRW.KillerName = playerName.ValueRO.Name;
                }
            }
        }
    }
#endif
}
