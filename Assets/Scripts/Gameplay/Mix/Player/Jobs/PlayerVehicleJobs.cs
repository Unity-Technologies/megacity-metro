using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using Unity.NetCode;
using UnityEngine;
using Utils.Misc;


namespace Unity.MegacityMetro.Gameplay
{
    [BurstCompile]
    [WithAll(typeof(Simulate))]
    internal partial struct ReconstructVehicleRotationJob : IJobEntity
    {
        public void Execute(in VehicleMovementState vehicleMovementState, ref LocalTransform localTransform)
        {
            localTransform.Rotation = vehicleMovementState.CalculateTargetRotation();
        }
    }
    
    [BurstCompile]
    [WithDisabled(typeof(GhostOwnerIsLocal))]
    internal partial struct InterpolatedVehicleRotationJob : IJobEntity
    {
        public void Execute(Entity entity, in VehicleMovementState vehicleMovementState, ref LocalTransform localTransform)
        {
            localTransform.Rotation = vehicleMovementState.CalculateTargetRotation();
        }
    }

    [BurstCompile]
    [WithAll(typeof(Simulate))]
    internal partial struct MoveJob : IJobEntity
    {
        public float DeltaTime;
        const float BaseSteeringSpeedMultiplier = 0.01f;

        public void Execute(
            in PlayerVehicleInput controlInput,
            in PlayerVehicleSettings vehicleSettings,
            ref PhysicsVelocity velocity,
            in VehicleHealth health,
            ref VehicleMovementState vehicleMovementState)
        {
            if (health.IsDead == 1)
            {
                // Gravity
                velocity.Linear += -math.up() * 20f * DeltaTime;
            }
            else
            {
                // Pitch & Yaw
                float2 smoothedLookVelocity = math.lerp(vehicleMovementState.LastLookVelocity, controlInput.LookVelocity, MathUtilities.GetSharpnessInterpolant(vehicleSettings.LookVelocitySharpness, DeltaTime));
                
                float rotationSpeed = BaseSteeringSpeedMultiplier * vehicleSettings.SteeringSpeed;
                float maxPitchRadians = math.radians(vehicleSettings.MaxPitchAngle);
                float yawDelta = (smoothedLookVelocity.y * rotationSpeed * DeltaTime);
                vehicleMovementState.PitchRadians = math.clamp(vehicleMovementState.PitchRadians + (smoothedLookVelocity.x * rotationSpeed * DeltaTime), -maxPitchRadians, maxPitchRadians);
                vehicleMovementState.YawRadians = vehicleMovementState.YawRadians + yawDelta;
                quaternion newRotation = vehicleMovementState.CalculateTargetRotation();
                float3 forward = math.mul(newRotation, math.forward());

                vehicleMovementState.LastLookVelocity = smoothedLookVelocity;
                
                // Acceleration
                {
                    float chosenAcceleration = vehicleSettings.Acceleration;
                    if (controlInput.Acceleration < 0f)
                    {
                        chosenAcceleration = vehicleSettings.Deceleration;
                    }
                    velocity.Linear += forward * controlInput.Acceleration * chosenAcceleration * DeltaTime;

                    float speed = math.length(velocity.Linear);
                    if (speed > vehicleSettings.MaxSpeed)
                    {
                        velocity.Linear = math.normalizesafe(velocity.Linear) * vehicleSettings.MaxSpeed;
                    }
                }

                // Roll
                {
                    // Accumulate roll when turning 
                    float yawSpeed = 0f;
                    if (DeltaTime != 0f)
                    {
                        yawSpeed = yawDelta / DeltaTime;
                        if (yawSpeed != 0f)
                        {
                            quaternion rotationWithoutRoll = vehicleMovementState.CalculateTargetRotationWithoutRoll();
                            float3 velChangeFromTurningDirection = math.mul(rotationWithoutRoll, math.right() * yawSpeed);
                            float3 newUp = math.mul(newRotation, math.up());
                            float dotUpWithVelChangeDirection = math.dot(math.normalizesafe(velChangeFromTurningDirection), newUp);

                            float turnAmount = -(1f - dotUpWithVelChangeDirection) * 0.5f * math.abs(yawSpeed);
                            float turnDirection = yawSpeed < 0f ? 1f : -1f;
                            vehicleMovementState.RollAccumulationFromTurn = math.clamp(vehicleMovementState.RollAccumulationFromTurn + (turnAmount * turnDirection * vehicleSettings.RollAccumulationSpeed), -1f, 1f);
                        }
                    }

                    // Base roll (from input)
                    vehicleMovementState.BaseRoll += (-controlInput.Roll * vehicleSettings.ManualRollSpeed * DeltaTime);
                    if (vehicleMovementState.BaseRoll > math.PI)
                    {
                        vehicleMovementState.BaseRoll -= math.PI * 2f;
                    }
                    else if (vehicleMovementState.BaseRoll < -math.PI)
                    {
                        vehicleMovementState.BaseRoll += math.PI * 2f;
                    }

                    // Final roll
                    vehicleMovementState.RollRadians = vehicleMovementState.BaseRoll + math.lerp(0f, math.PI * 0.5f, -vehicleMovementState.RollAccumulationFromTurn);

                    // Restitution of base roll
                    if (math.abs(controlInput.Roll) < 0.001f)
                    {
                        vehicleMovementState.BaseRoll = math.lerp(vehicleMovementState.BaseRoll, 0f, MathUtilities.GetSharpnessInterpolant(vehicleSettings.RollRestitutionSpeed, DeltaTime));
                    }

                    // Restitution of turning roll
                    if (vehicleSettings.YawSpeedForMaxRoll != 0f)
                    {
                        float finalRollRestitutionSpeed = math.lerp(vehicleSettings.RollRestitutionSpeed, 0f, math.saturate(math.abs(yawSpeed) / vehicleSettings.YawSpeedForMaxRoll));
                        vehicleMovementState.RollAccumulationFromTurn = math.lerp(vehicleMovementState.RollAccumulationFromTurn, 0f, MathUtilities.GetSharpnessInterpolant(finalRollRestitutionSpeed, DeltaTime));
                    }
                }
            }
        }
    }

    [BurstCompile]
    [WithAll(typeof(Simulate))]
    internal partial struct HealthAndImmunityJob : IJobEntity
    {
        public float DeltaTime;

        public void Execute(
            ref VehicleHealth health,
            ref Immunity immunity)
        {
            if (immunity.Counter > 0f)
            {
                immunity.Counter -= DeltaTime;
            }
            
            // Death
            // Add a threshold to avoid the death UI
            if (math.abs(health.Value) < 0.01f && health.IsDead == 0)
            {
                health.IsDead = 1;
                health._deathTimer = 5f;
            }

            if (health.IsDead == 1)
            {
                health._deathTimer -= DeltaTime;

                // Revive
                if (health._deathTimer <= 0)
                {
                    health.Value = 100;
                    health.IsDead = 0;

                    immunity.Counter = immunity.Duration;
                }
            }
        }
    }

    [BurstCompile]
    [WithAll(typeof(Simulate))]
    internal partial struct ApplyVehicleRotationAsVelocityJob : IJobEntity
    {
        public float DeltaTime;

        public void Execute(
            ref PhysicsVelocity physicsVelocity,
            in VehicleMovementState vehicleMovementState,
            in PhysicsMass mass,
            in LocalTransform localTransform)
        {
            physicsVelocity.Angular = PhysicsVelocity.CalculateVelocityToTarget(in mass, localTransform.Position, localTransform.Rotation, new RigidTransform(vehicleMovementState.CalculateTargetRotation(), localTransform.Position), 1f / DeltaTime).Angular;
        }
    }
}