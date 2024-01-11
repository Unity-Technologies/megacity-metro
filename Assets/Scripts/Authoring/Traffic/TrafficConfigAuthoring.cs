using UnityEngine;
using Unity.Entities;
using System.Collections.Generic;

namespace Unity.MegacityMetro.Traffic
{
    public class TrafficConfigAuthoring : MonoBehaviour
    {
        public bool enableTraffic = true;
        public float globalCarSpeed = 1.0f;
        public int maxVehiclesPerRoad = 1000;
        public int maxVehiclesTotal = 10000;
        public float laneOffsetScale = 4;
        public float bankingMax = 0.3f;
        public float minDistanceBetweenVehicles = 30;
        public float maxDistanceBetweenVehicles = 200;
        public List<GameObject> vehiclePrefabs;

        [BakingVersion("megacity-metro", 2)]
        public class Baker : Baker<TrafficConfigAuthoring>
        {
            public override void Bake(TrafficConfigAuthoring authoring)
            {
                for (int j = 0; j < authoring.vehiclePrefabs.Count; j++)
                {
                    Entity vehiclePrefab = CreateAdditionalEntity(TransformUsageFlags.Dynamic);
                    var prefabEntity = GetEntity(authoring.vehiclePrefabs[j], TransformUsageFlags.Dynamic);
                    var prefabData = new VehiclePrefabRef
                    {
                        VehiclePrefab = prefabEntity,
                    };

                    AddComponent(vehiclePrefab, prefabData);
                }

                var trafficSettings = new TrafficConfig
                {
                    GlobalCarSpeed = authoring.globalCarSpeed,
                    MaxVehiclesPerRoad = authoring.maxVehiclesPerRoad,
                    MaxVehiclesTotal = authoring.maxVehiclesTotal,
                    LaneOffsetScale = authoring.laneOffsetScale,
                    BankingMax = authoring.bankingMax,
                    MinDistanceBetweenVehicles = authoring.minDistanceBetweenVehicles,
                    MaxDistanceBetweenVehicles = authoring.maxDistanceBetweenVehicles
                };
                var entity = GetEntity(authoring.gameObject, TransformUsageFlags.Dynamic);
                AddComponent(entity, trafficSettings);

                if (authoring.enableTraffic)
                {
                    AddComponent<TrafficEnabled>(entity);
                }
            }
        }
    }
}