using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using Utils.Misc;

namespace Unity.MegacityMetro.Gameplay
{
    [BurstCompile]
    public partial struct LaserVisualJob : IJobEntity
    {
        [ReadOnly] 
        public ComponentLookup<LocalToWorld> LocalToWorldLookup;
        [ReadOnly] 
        public CollisionWorld CollisionWorld;

        [BurstCompile]
        private void Execute(ref VehicleLaser laser, in Entity entity, in DynamicBuffer<WeaponShotIgnoredEntity> ignoredEntities)
        {
            laser.ShowHitVFX = false;
            laser.VFXLaserStartNode = default;
            laser.VFXLaserEndNode = default;
            
            if (!laser.IsShooting || laser.Energy <= 0) 
                return;

            var ltw = LocalToWorldLookup[entity];
            var startPoint = laser.CalculateLaserStartPoint(ltw.Position, ltw.Rotation);
            var endPoint = laser.CalculateLaserEndPoint(ltw.Position, ltw.Rotation, ref LocalToWorldLookup);

            if (laser.DetectedTarget == Entity.Null)
            {
                var ray = new RaycastInput
                {
                    Start = startPoint,
                    End = endPoint,
                    Filter = laser.LaserObstructionsFilter,
                };
                ClosestRaycastCollisionWithIgnoredEntitiesCollector collector = new ClosestRaycastCollisionWithIgnoredEntitiesCollector(ignoredEntities.Reinterpret<Entity>().AsNativeArray());
                if (CollisionWorld.CastRay(ray, ref collector) && collector.ClosestHit.Entity != Entity.Null)
                {
                    endPoint = collector.ClosestHit.Position;
                    laser.ShowHitVFX = true;
                }   
            }
            else
            {
                laser.ShowHitVFX = true;
            }

            laser.VFXLaserStartNode = startPoint;
            laser.VFXLaserEndNode = endPoint;
        }
    }
}