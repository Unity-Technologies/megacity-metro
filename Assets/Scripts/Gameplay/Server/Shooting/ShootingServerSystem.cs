using Unity.Burst;
using Unity.Entities;
using Unity.NetCode.Extensions;
using Unity.Transforms;

namespace Unity.MegacityMetro.Gameplay
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct ShootingServerSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var damageJob = new DamageJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
                PlayerNameLookup = SystemAPI.GetComponentLookup<PlayerName>(true),
                ImmunityLookup = SystemAPI.GetComponentLookup<Immunity>(true),
                HealthLookup = SystemAPI.GetComponentLookup<VehicleHealth>(false),
                PlayerScoreLookup = SystemAPI.GetComponentLookup<PlayerScore>(false),
                LaserLookup = SystemAPI.GetComponentLookup<VehicleLaser>(false),
                LocalToWorldLookup = SystemAPI.GetComponentLookup<LocalToWorld>(true),

            };
            
            state.Dependency = damageJob.Schedule(state.Dependency);
        }
    }
}