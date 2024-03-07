using Unity.Entities;
using Unity.Physics;

namespace Unity.MegacityMetro.Gameplay
{
    /// <summary>
    /// Set of components required for player vehicle movement and control
    /// </summary>
    public struct PlayerVehicleSettings : IComponentData
    {
        public float Acceleration;
        public float Deceleration;
        public float MaxSpeed;
        public float MaxPitchAngle;
        public float SteeringSpeed;
        public float LookVelocitySharpness;
        
        public float RollAccumulationSpeed;
        public float ManualRollSpeed;
        public float RollRestitutionSpeed;
        public float YawSpeedForMaxRoll;
        
        public bool InvertPitch;
        public float TargetFollowDamping;
        public float TargetSqLerpThreshold;
        
        public Entity VehicleFX;
    }
}
