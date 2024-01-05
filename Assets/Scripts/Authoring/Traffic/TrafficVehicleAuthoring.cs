using Unity.Entities;
using UnityEngine;

namespace Unity.MegacityMetro.Traffic
{
    public class TrafficVehicleAuthoring : MonoBehaviour
    {
        [BakingVersion("megacity-metro", 2)]
        public class Baker : Baker<TrafficVehicleAuthoring>
        {
            public override void Bake(TrafficVehicleAuthoring authoring)
            {
                var entity = GetEntity(authoring.gameObject, TransformUsageFlags.Renderable);
                AddComponent<VehiclePathing>(entity);
            }
        }
    }
}
