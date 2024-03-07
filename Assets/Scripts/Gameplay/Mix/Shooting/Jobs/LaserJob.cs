using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using Utils.Misc;

namespace Unity.MegacityMetro.Gameplay
{
    /// <summary>
    /// Job to simulate the laser.
    /// </summary>
    [BurstCompile]
    [WithAll(typeof(Simulate))]
    public partial struct LaserJob : IJobEntity
    {
        [ReadOnly]
        public NetworkTick PredictingTick;
        [ReadOnly]
        public float DeltaTime;
        [ReadOnly]
        public PhysicsWorldHistorySingleton CollisionHistory;
        [ReadOnly]
        public PhysicsWorld PhysicsWorld;
        [ReadOnly]
        public ComponentLookup<VehicleHealth> HealthLookup;
        [ReadOnly]
        public ComponentLookup<Immunity> ImmunityLookup;
        
        [BurstCompile]
        private void Execute(
            in Entity entity, 
            ref VehicleLaser laser, 
            in LocalTransform vehicleTransform,
            in PlayerVehicleInput input,
            in CommandDataInterpolationDelay interpolationDelay,
            in DynamicBuffer<WeaponShotIgnoredEntity> ignoredEntities)
        {
            laser.DetectedTarget = default;
            HealthLookup.TryGetComponent(entity, out VehicleHealth ownerHealth);
            
            if (ownerHealth.IsDead == 1)
            {
                laser.IsShooting = false;
            }
            else
            {
                // Calculate laser vector
                {
                    float3 vehicleForward = math.mul(vehicleTransform.Rotation, math.forward());
                    float3 worldLaserStart = laser.CalculateLaserStartPoint(vehicleTransform.Position, vehicleTransform.Rotation);
                    NativeArray<Entity> ignoredEntitiesArray = ignoredEntities.Reinterpret<Entity>().AsNativeArray();
                    CollisionHistory.GetCollisionWorldFromTick(PredictingTick, interpolationDelay.Delay, ref PhysicsWorld, out var collisionWorld);

                    float coneAnglesRadians = math.radians(laser.AutoAimDegrees) * input.AimAssistSensitivity;
                    float radiusAtMaxLength = MathUtilities.GetConeRadiusAtLength(laser.Range, coneAnglesRadians);
                    
                    AutoAimHitsCollector collector = new AutoAimHitsCollector(
                        in collisionWorld, 
                        in HealthLookup, 
                        in ImmunityLookup,
                        laser.LaserObstructionsFilter, 
                        ignoredEntitiesArray, 
                        worldLaserStart, 
                        vehicleForward, 
                        coneAnglesRadians, 
                        laser.AutoAimMinRadius,
                        MathUtilities.AngleRadiansToDotRatio(math.radians(70f)));
                    collisionWorld.SphereCastCustom(worldLaserStart, radiusAtMaxLength, vehicleForward, laser.Range, ref collector, laser.VehiclesFilter);
                    if (collector.ChosenHit.Entity != Entity.Null)
                    {
                        laser.DetectedTarget = collector.ChosenHit.Entity;
                    }
                }

                // Handle shooting
                bool hasEnergyToShoot = laser.Energy > 0 || ownerHealth.IsDead == 0;
                laser.IsShooting = input.Shoot && hasEnergyToShoot;
                if (laser.IsShooting)
                {
                    // Energy drain when shooting
                    laser.Energy = math.max(0, laser.Energy - (DeltaTime * laser.DrainSpeed));
                }
                else
                {
                    // Energy recharge
                    laser.Energy = math.min(100, laser.Energy + (DeltaTime * laser.ChargerSpeed));
                }
            }
        }
    }
    
    [BurstCompile]
    public struct AutoAimHitsCollector : ICollector<ColliderCastHit>
    {
        public bool EarlyOutOnFirstHit => false;
        public float MaxFraction => 1f;
        public int NumHits => ChosenHit.Entity != Entity.Null ? 1 : 0;
        public ColliderCastHit ChosenHit;
        
        private CollisionWorld m_CollisionWorld;
        private CollisionFilter m_ObstructionsFilter;
        private NativeArray<Entity> m_IgnoredEntities;
        private float m_DotRatioForConeAngle;
        private float3 m_LaserStart;
        private float3 m_LaserForward;
        private float m_MinRadius;
        private float m_MinDotRatio;
        private ComponentLookup<VehicleHealth> m_VehicleHealthLookup;
        private ComponentLookup<Immunity> m_ImmunityLookup;

        public AutoAimHitsCollector(
            in CollisionWorld collisionWorld, 
            in ComponentLookup<VehicleHealth> vehicleHealthLookup, 
            in ComponentLookup<Immunity> immunityLookup, 
            CollisionFilter obstructionsFilter, 
            NativeArray<Entity> ignoredEntities, 
            float3 laserStart, 
            float3 laserForward, 
            float coneAngleRadians, 
            float minRadius,
            float minDotRatio)
        {
            ChosenHit = default;
            ChosenHit.Fraction = float.MaxValue;
            
            m_CollisionWorld = collisionWorld;
            m_ObstructionsFilter = obstructionsFilter;
            m_IgnoredEntities = ignoredEntities;
            m_DotRatioForConeAngle = MathUtilities.AngleRadiansToDotRatio(coneAngleRadians);
            m_LaserStart = laserStart;
            m_LaserForward = laserForward;
            m_MinRadius = minRadius;
            m_MinDotRatio = minDotRatio;
            m_VehicleHealthLookup = vehicleHealthLookup;
            m_ImmunityLookup = immunityLookup;
        }

        public bool AddHit(ColliderCastHit hit)
        {
            if (IsEntityIgnored(hit.Entity))
            {
                return false;
            }
            
            // Ignore anything farther than our latest chosen hit
            if (hit.Fraction >= ChosenHit.Fraction)
            {
                return false;
            }

            if (!PhysicsUtilities.IsCollidable(hit.Material))
            {
                return false;
            }

            float3 effectiveHitPoint = hit.Position;
            if (m_CollisionWorld.Bodies.Length > hit.RigidBodyIndex)
            {
                effectiveHitPoint = m_CollisionWorld.Bodies[hit.RigidBodyIndex].WorldFromBody.pos;
            }

            // Check hit is in cone
            {
                float3 vectorToHit = effectiveHitPoint - m_LaserStart;
                float distanceFromCenter = math.length(vectorToHit - math.projectsafe(vectorToHit, m_LaserForward));
                float3 directionToHit = math.normalizesafe(vectorToHit);
                float dotRatio = math.dot(m_LaserForward, directionToHit);

                // If hit is behind, ignore
                if (dotRatio < m_MinDotRatio)
                {
                    return false;
                }
                
                // if farther than min radius, check cone
                if (distanceFromCenter > m_MinRadius)
                {
                    // If hit is outside of cone, ignore
                    if (dotRatio < m_DotRatioForConeAngle)
                    {
                        return false;
                    }
                }
            }

            // Check health
            if (m_VehicleHealthLookup.TryGetComponent(hit.Entity, out VehicleHealth hitHealth))
            {
                if (hitHealth.IsDead == 1)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            // Check invincibility
            if (m_ImmunityLookup.TryGetComponent(hit.Entity, out Immunity immunity))
            {
                if (immunity.Counter > 0f)
                {
                    return false;
                }
            }

            // Check obstructions
            {
                RaycastInput rayInput = new RaycastInput
                {
                    Start = m_LaserStart,
                    End = effectiveHitPoint,
                    Filter = m_ObstructionsFilter,
                };
                ClosestRaycastCollisionWithIgnoredEntitiesCollector obstructionsCollector = new ClosestRaycastCollisionWithIgnoredEntitiesCollector(in m_IgnoredEntities);
                if (m_CollisionWorld.CastRay(rayInput, ref obstructionsCollector))
                {
                    // Detect obstruction
                    if (obstructionsCollector.ClosestHit.Entity != Entity.Null && obstructionsCollector.ClosestHit.Entity != hit.Entity)
                    {
                        return false;
                    }
                }

                ChosenHit = hit;
            }

            return true;
        }

        private bool IsEntityIgnored(Entity entity)
        {
            for (int i = 0; i < m_IgnoredEntities.Length; i++)
            {
                if(m_IgnoredEntities[i] == entity)
                {
                    return true;
                }
            }
            return false;
        }
    }
}