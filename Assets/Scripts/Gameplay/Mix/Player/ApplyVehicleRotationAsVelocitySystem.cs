using Unity.Burst;
using Unity.Entities;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace Unity.MegacityMetro.Gameplay
{
    [BurstCompile]
    [UpdateInGroup(typeof(BeforePhysicsSystemGroup), OrderFirst = true)]
    public partial struct ReconstructVehicleRotationSystem : ISystem
    {
        private EntityQuery m_vehicleQuery;

        public void OnCreate(ref SystemState state)
        {
            m_vehicleQuery = new EntityQueryBuilder(state.WorldUpdateAllocator)
                .WithAll<PlayerVehicleInput>()
                .Build(ref state);
            state.RequireForUpdate(m_vehicleQuery);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var job = new ReconstructVehicleRotationJob();
            state.Dependency = job.ScheduleParallel(state.Dependency);
        }
    }
    
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(TransformSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct InterpolatedVehicleRotationSystem : ISystem
    {
        private EntityQuery m_vehicleQuery;

        public void OnCreate(ref SystemState state)
        {
            m_vehicleQuery = new EntityQueryBuilder(state.WorldUpdateAllocator)
                .WithAll<PlayerVehicleInput>()
                .Build(ref state);
            state.RequireForUpdate(m_vehicleQuery);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var job = new InterpolatedVehicleRotationJob();
            state.Dependency = job.ScheduleParallel(state.Dependency);
        }
    }

    /// <summary>
    /// Schedule the necessary job to process the user inputs and move the player accordingly
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(BeforePhysicsSystemGroup), OrderLast = true)]
    public partial struct ApplyVehicleRotationAsVelocitySystem : ISystem
    {
        private EntityQuery m_vehicleQuery;

        public void OnCreate(ref SystemState state)
        {
            m_vehicleQuery = new EntityQueryBuilder(state.WorldUpdateAllocator)
                .WithAll<PlayerVehicleInput>()
                .Build(ref state);
            state.RequireForUpdate(m_vehicleQuery);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = state.WorldUnmanaged.Time.DeltaTime;
            var velocityRotationJob = new ApplyVehicleRotationAsVelocityJob { DeltaTime = deltaTime };
            state.Dependency = velocityRotationJob.ScheduleParallel(state.Dependency);
        }
    }
}