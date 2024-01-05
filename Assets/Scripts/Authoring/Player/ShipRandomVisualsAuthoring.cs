using UnityEngine;
using Unity.Entities;

public class ShipRandomVisualsAuthoring : MonoBehaviour
{
    public GameObject LEGEntity;
    public GameObject[] ShipVisuals;

    [BakingVersion("megacity-metro", 1)]
    public class Baker : Baker<ShipRandomVisualsAuthoring>
    {
        public override void Bake(ShipRandomVisualsAuthoring authoring)
        {
            var entity = GetEntity(authoring.gameObject, TransformUsageFlags.Dynamic);

            AddComponent(entity, new RootEntity
            {
                Entity = GetEntity(authoring.LEGEntity, TransformUsageFlags.Dynamic),
            });

            var buffer = AddBuffer<ShipRandomVisuals>(entity);
            foreach (GameObject prefab in authoring.ShipVisuals)
            {
                buffer.Add(new ShipRandomVisuals { ShipVisual = GetEntity(prefab, TransformUsageFlags.Dynamic) });
            }
        }
    }
}