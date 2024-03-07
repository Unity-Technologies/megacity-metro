using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using static Unity.Entities.SystemAPI;

namespace Unity.MegacityMetro.Gameplay
{
    /// <summary>
    /// System to handle the player vehicle collision.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(AfterPhysicsSystemGroup))]
    partial struct PlayerVehicleCollisionSystem : ISystem
    {
        private ComponentLookup<PhysicsVelocity> _physicsVelocities;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate<SimulationSingleton>();
            _physicsVelocities = state.GetComponentLookup<PhysicsVelocity>(false);
        }


        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var physicsWorldSingleton = GetSingleton<PhysicsWorldSingleton>();
            var simulationSingleton = GetSingleton<SimulationSingleton>();
            _physicsVelocities.Update(ref state);
            
            state.Dependency = new PlayerVehicleCollisionJob
            {
                SimulateLookup = SystemAPI.GetComponentLookup<Simulate>(true),
                Bodies = physicsWorldSingleton.Bodies,
                PhysicsVelocities = _physicsVelocities,
            }.Schedule(simulationSingleton, state.Dependency);
        }
    }
}
