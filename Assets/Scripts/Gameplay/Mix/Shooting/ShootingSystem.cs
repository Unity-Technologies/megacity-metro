using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using Unity.Physics;
using static Unity.Entities.SystemAPI;

namespace Unity.MegacityMetro.Gameplay
{
    /// <summary>
    /// Handles shooting
    /// </summary>
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    public partial struct ShootingSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkTime>();
            state.RequireForUpdate<PhysicsWorldHistorySingleton>();
            state.RequireForUpdate<PhysicsWorldSingleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var netTime = GetSingleton<NetworkTime>();
            var laserJob = new LaserJob
            {
                CollisionHistory = GetSingleton<PhysicsWorldHistorySingleton>(),
                PhysicsWorld = GetSingleton<PhysicsWorldSingleton>().PhysicsWorld,
                DeltaTime = Time.DeltaTime,
                PredictingTick = netTime.ServerTick,
                HealthLookup = GetComponentLookup<VehicleHealth>(true),
                ImmunityLookup = GetComponentLookup<Immunity>(true),
            };
            state.Dependency = laserJob.ScheduleParallel(state.Dependency);
        }
    }
}