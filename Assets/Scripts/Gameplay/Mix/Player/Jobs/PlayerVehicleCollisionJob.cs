using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

namespace Unity.MegacityMetro.Gameplay
{
    /// <summary>
    /// This system handles collision events for rigid bodies with the PlayerVehicle custom tag
    /// </summary>
    [BurstCompile]
    struct PlayerVehicleCollisionJob : ICollisionEventsJob
    {
        [ReadOnly] public ComponentLookup<Simulate> SimulateLookup;
        [ReadOnly] public NativeSlice<RigidBody> Bodies;
        public ComponentLookup<PhysicsVelocity> PhysicsVelocities;

        public void Execute(CollisionEvent collisionEvent)
        {
            TryCutThrottleIfPlayerVehicle(collisionEvent.BodyIndexA, collisionEvent.EntityA, collisionEvent.Normal);
            TryCutThrottleIfPlayerVehicle(collisionEvent.BodyIndexB, collisionEvent.EntityB, collisionEvent.Normal);
        }

        bool TryCutThrottleIfPlayerVehicle(int bodyIndex, Entity entity, float3 normal)
        {
            if (!SimulateLookup.IsComponentEnabled(entity))
                return false;
            // custom body tag 0 is for PlayerVehicle
            // vehicles with this tag are expected to have PlayerVehicleState and PhysicsVelocity
            if ((Bodies[bodyIndex].CustomTags & (1 << 0)) == 0)
            {
                // TODO: play a sound if one of the bodies was a player car
                return false;
            }
            var velocity = PhysicsVelocities[entity];
            // cut thrust in proportion to angle angle of attack
            var scalar = 1f - math.abs(math.dot(normal, math.normalizesafe(PhysicsVelocities[entity].Linear)));
            velocity.Linear *= scalar;
            PhysicsVelocities[entity] = velocity;
            return true;
        }
    }
}
