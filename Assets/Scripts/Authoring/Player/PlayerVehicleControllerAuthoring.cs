using Unity.Entities;
using Unity.Mathematics;
using Unity.MegacityMetro.Gameplay;
using UnityEngine;
using Utils.Misc;

namespace Unity.MegacityMetro.Authoring
{
    /// <summary>
    /// Create all the required player vehicle components
    /// </summary>
    public class PlayerVehicleControllerAuthoring : MonoBehaviour
    {
        [Header("Speed")]
        public float Acceleration = 5.0f;
        public float Deceleration = 10.0f;
        public float MaxSpeed = 50f;

        [Header("Rotation")]
        public float SteeringSpeed = 5.0f;
        public float LookVelocitySharpness = 10.0f;
        public bool InvertPitch;
        public float MaxPitchAngle = 89f;
        public float RollAccumulationSpeed = 0.03f;
        public float ManualRollSpeed = 5f;
        public float RollRestitutionSpeed = 2f;
        public float YawSpeedForMaxRoll = 1.8f;

        [Header("Camera")]
        public float TargetFollowDamping = 2.0f;
        public float TargetSqLerpThreshold = 2500;

        [Header("Laser")]
        public LayerMask VehiclesPhysicsCategory;
        public LayerMask LaserObstructionsPhysicsCategory;
        public float DamagePerSecond = 50f;
        public float InitialEnergy = 100f;
        public float ChargeSpeed = 2.5f;
        public float DrainSpeed = 3f;
        public float AutoAimDegrees = 20f;
        public float AutoAimMinRadius = 3f;
        public float LaserRange = 150f;

        [Header("Immunity")]
        public float ImmunityDuration = 6.0f;

        [BakingVersion("megacity-metro", 5)]
        public class PlayerVehicleControllerBaker : Baker<PlayerVehicleControllerAuthoring>
        {
            public override void Bake(PlayerVehicleControllerAuthoring authoring)
            {
                var entity = GetEntity(authoring.gameObject, TransformUsageFlags.Dynamic);

                AddComponent(entity, new VehicleLaser
                {
                    DamagePerSecond = authoring.DamagePerSecond,
                    Energy = authoring.InitialEnergy,
                    ChargerSpeed = authoring.ChargeSpeed,
                    DrainSpeed = authoring.DrainSpeed,
                    AutoAimDegrees = authoring.AutoAimDegrees,
                    AutoAimMinRadius = authoring.AutoAimMinRadius,
                    Range = authoring.LaserRange,

                    VehiclesFilter = PhysicsUtilities.ProduceCollisionFilter(authoring.VehiclesPhysicsCategory.value),
                    LaserObstructionsFilter = PhysicsUtilities.ProduceCollisionFilter(authoring.LaserObstructionsPhysicsCategory.value),
                });


                var ignoreLaser = AddBuffer<WeaponShotIgnoredEntity>(entity);
                ignoreLaser.Add(new WeaponShotIgnoredEntity { Value = entity });

                AddComponent(entity, new VehicleHealth { Value = 100 });
                AddComponent(entity, new PlayerScore { Value = 0 });
                AddComponent(entity, new Immunity { Duration = authoring.ImmunityDuration });
                // Add player inputs component
                var input = new PlayerVehicleInput
                {
                    LookVelocity = float2.zero,
                    Acceleration = 0,
                    Roll = 0
                };
                AddComponent(entity, input);

                // Add vehicle settings component
                var settings = new PlayerVehicleSettings
                {
                    Acceleration = authoring.Acceleration,
                    Deceleration = authoring.Deceleration,
                    MaxSpeed = authoring.MaxSpeed,
                    MaxPitchAngle = authoring.MaxPitchAngle,
                    SteeringSpeed = authoring.SteeringSpeed,
                    LookVelocitySharpness = authoring.LookVelocitySharpness,

                    RollAccumulationSpeed = authoring.RollAccumulationSpeed,
                    ManualRollSpeed = authoring.ManualRollSpeed,
                    RollRestitutionSpeed = authoring.RollRestitutionSpeed,
                    YawSpeedForMaxRoll = authoring.YawSpeedForMaxRoll,

                    InvertPitch = authoring.InvertPitch,
                    TargetFollowDamping = authoring.TargetFollowDamping,
                    TargetSqLerpThreshold = authoring.TargetSqLerpThreshold,
                };
                AddComponent(entity, settings);

                VehicleMovementState vehicleState = new VehicleMovementState();
                VehicleMovementState.SetRotation(ref vehicleState, authoring.transform.rotation);
                AddComponent(entity, vehicleState);
            }
        }
    }
}
