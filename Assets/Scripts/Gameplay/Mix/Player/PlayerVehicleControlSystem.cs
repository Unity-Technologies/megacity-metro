using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using Unity.Physics.Systems;

namespace Unity.MegacityMetro.Gameplay
{
    /// <summary>
    /// Schedule the necessary job to process the user inputs and move the player accordingly
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(BeforePhysicsSystemGroup))]
    public partial struct PlayerVehicleControlSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            var moveJob = new MoveJob {DeltaTime = deltaTime};
            state.Dependency = moveJob.ScheduleParallel(state.Dependency);
        }
    }
    
    [BurstCompile]
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct VehicleHealthAndImmunitySystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            var job = new HealthAndImmunityJob {DeltaTime = deltaTime};
            state.Dependency = job.ScheduleParallel(state.Dependency);
        }
    }
}