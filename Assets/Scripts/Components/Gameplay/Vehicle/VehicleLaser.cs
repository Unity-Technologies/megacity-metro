using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using Utils.Misc;

namespace Unity.MegacityMetro.Gameplay
{
    /// <summary>
    ///  Vehicle laser settings
    /// </summary>
    public struct VehicleLaser : IComponentData
    {
        public float DamagePerSecond;
        public float Range;
        public float ChargerSpeed;
        public float DrainSpeed;
        public float AutoAimDegrees;
        public float AutoAimMinRadius;
        public float3 LocalLaserStartPoint;
        public CollisionFilter VehiclesFilter;
        public CollisionFilter LaserObstructionsFilter;
        
        public float3 VFXLaserStartNode;
        public float3 VFXLaserEndNode;
        public bool ShowHitVFX;
        
        [GhostField(Quantization = 1000)] 
        public float Energy;
        [GhostField] 
        public bool IsShooting;
        [GhostField]
        public Entity DetectedTarget;
        

        public readonly float3 CalculateLaserStartPoint(float3 vehiclePosition, quaternion vehicleRotation)
        {
            return math.transform(float4x4.TRS(vehiclePosition, vehicleRotation, 1f), LocalLaserStartPoint);
        }
        
        public readonly float3 CalculateLaserEndPoint(float3 vehiclePosition, quaternion vehicleRotation, ref ComponentLookup<LocalToWorld> localToWorldLookup, bool skipDetectedTarget = false)
        {
            if (!skipDetectedTarget &&
                DetectedTarget != Entity.Null && 
                localToWorldLookup.TryGetComponent(DetectedTarget, out LocalToWorld targetLtW))
            {
                return targetLtW.Position;
            }
            else
            {
                return CalculateLaserStartPoint(vehiclePosition, vehicleRotation) + math.mul(vehicleRotation, math.forward() * Range);
            }
        }
    }
}