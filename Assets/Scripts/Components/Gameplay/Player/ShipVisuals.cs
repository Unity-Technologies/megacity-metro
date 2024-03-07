using Unity.Entities;
using Unity.Mathematics;

public struct ShipVisuals : IComponentData
{
    public Entity FXEntity;
    public float3 LocalLaserStartPoint;
}