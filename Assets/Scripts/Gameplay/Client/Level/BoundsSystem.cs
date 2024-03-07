using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;

namespace Unity.MegacityMetro.Gameplay
{
    /// <summary>
    /// System that updates the player location bounds.
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.LocalSimulation)]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial struct UpdateBoundsSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<LevelBounds>();
            state.RequireForUpdate<PlayerLocationBounds>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (physicsCollider, 
                         localTransform, 
                         levelBounds) in SystemAPI.Query<
                         RefRO<PhysicsCollider>, 
                         RefRO<LocalTransform>,
                         RefRO<LevelBounds>>())
            {
                var checkStateIsInsideBoundsJob = new CheckStateIsInsideBoundsJob
                {
                    MaxDistance = levelBounds.ValueRO.MaxDistance,
                    LevelBoundsPhysicsCollider = physicsCollider.ValueRO,
                    LevelBoundsInvMatrix = localTransform.ValueRO.ToInverseMatrix()
                };
            
                state.Dependency = checkStateIsInsideBoundsJob.Schedule(state.Dependency);
                break;
            }
        }
    }

    [BurstCompile]
    [WithAll(typeof(PlayerVehicleInput))]
    public partial struct CheckStateIsInsideBoundsJob : IJobEntity
    {
        [ReadOnly] 
        public float MaxDistance;
        [ReadOnly]
        public PhysicsCollider LevelBoundsPhysicsCollider;
        [ReadOnly]
        public float4x4 LevelBoundsInvMatrix;
       
        [BurstCompile]
        private void Execute(ref PlayerLocationBounds playerLocationBounds, in LocalTransform vehicleTransform)
        {
            // Reset bounds
            playerLocationBounds.IsInside = false;
            var localVehiclePositionRelativelyToLevelBounds = math.transform(LevelBoundsInvMatrix, vehicleTransform.Position);
            var pointDistanceInput = new PointDistanceInput
            {    
                Position = localVehiclePositionRelativelyToLevelBounds,
                MaxDistance = MaxDistance, 
                Filter = CollisionFilter.Default,
            };
            
            unsafe
            {
                if (LevelBoundsPhysicsCollider.ColliderPtr->CalculateDistance(pointDistanceInput, out DistanceHit distanceHit))
                {
                    playerLocationBounds.IsInside = distanceHit.Distance < 0f;
                }
            }
        }
    }
}
