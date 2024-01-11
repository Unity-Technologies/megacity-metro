using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Utils.Misc;

[GhostComponent]
public struct VehicleMovementState : IComponentData
{
    [GhostField(Quantization = 1000, Smoothing = SmoothingAction.Interpolate)] 
    public float PitchRadians;
    [GhostField(Quantization = 1000, Smoothing = SmoothingAction.Interpolate)] 
    public float YawRadians;
    [GhostField(Quantization = 1000, Smoothing = SmoothingAction.Interpolate)] 
    public float RollRadians;
    
    [GhostField(Quantization = 1000, Smoothing = SmoothingAction.Interpolate)] 
    public float BaseRoll;
    [GhostField(Quantization = 1000, Smoothing = SmoothingAction.Interpolate)] 
    public float RollAccumulationFromTurn;
    
    [GhostField(Quantization = 1000, Smoothing = SmoothingAction.Interpolate)] 
    public float2 LastLookVelocity;

    public quaternion CalculateTargetRotation()
    {
        return quaternion.Euler(PitchRadians, YawRadians, RollRadians);
    }

    public quaternion CalculateTargetRotationWithoutRoll()
    {
        return quaternion.Euler(PitchRadians, YawRadians, 0f);
    }

    public static void SetRotation(ref VehicleMovementState vehicleMovementState, quaternion targetRotation)
    {
        float3 eulerAngles = MathUtilities.ToEuler(targetRotation);
        vehicleMovementState.PitchRadians = eulerAngles.x;
        vehicleMovementState.YawRadians = eulerAngles.y;
        vehicleMovementState.RollRadians = eulerAngles.z;
    }
}
