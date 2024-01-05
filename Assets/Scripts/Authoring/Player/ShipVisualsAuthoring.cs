using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class ShipVisualsAuthoring : MonoBehaviour
{
    public GameObject FXEntity;
    public GameObject LaserRoot;

    [BakingVersion("megacity-metro", 1)]
    class Baker : Baker<ShipVisualsAuthoring>
    {
        public override void Bake(ShipVisualsAuthoring authoring)
        {
            var entity = GetEntity(authoring.gameObject, TransformUsageFlags.Dynamic);

            float4x4 vehicleTransform = float4x4.TRS(authoring.transform.position, authoring.transform.rotation, authoring.transform.localScale.x);
            float4x4 laserRootTransform = float4x4.TRS(authoring.LaserRoot.transform.position, authoring.LaserRoot.transform.rotation, 1f);

            AddComponent(entity, new ShipVisuals
            {
                FXEntity = GetEntity(authoring.FXEntity, TransformUsageFlags.WorldSpace | TransformUsageFlags.Dynamic),
                LocalLaserStartPoint = math.mul(math.inverse(vehicleTransform), laserRootTransform).Translation(),
            });
        }
    }
}
