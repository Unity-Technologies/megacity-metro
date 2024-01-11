using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Splines;
using Random = Unity.Mathematics.Random;

namespace Unity.MegacityMetro.Traffic
{
    [UpdateBefore(typeof(TransformSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.LocalSimulation)]
    public partial struct TrafficSetupSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<VehiclePrefabRef>();
            state.RequireForUpdate<TrafficConfig>();
            state.RequireForUpdate<TrafficEnabled>();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            foreach (var road in
                     SystemAPI.Query<RefRW<Road>>())
            {
                road.ValueRW.Spline.Dispose();
            }
        }

        public void OnUpdate(ref SystemState state)
        {
            Debug.Log("building roads and spawning cars");

            var trafficConfig = SystemAPI.GetSingleton<TrafficConfig>();
            var vehiclePrefabQuery = SystemAPI.QueryBuilder().WithAll<VehiclePrefabRef>().Build();
            var vehiclePrefabs = vehiclePrefabQuery.ToComponentDataArray<VehiclePrefabRef>(Allocator.Temp);
            if (vehiclePrefabs.Length == 0)
            {
                return;
            }

            var random = new Mathematics.Random(123);
            var roadIndex = 0;
            var totalVehicles = 0;

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (offset, roadPoints) in
                     SystemAPI.Query<RefRO<RoadOffset>, DynamicBuffer<RoadPoint>>())
            {
                var road = new Spline();
                for (int i = 0; i < roadPoints.Length; i++)
                {
                    var point = roadPoints[i].Value + offset.ValueRO.Value;
                    road.Add(new BezierKnot(point));
                }

                // auto smooth the tangents
                var range = new SplineRange(0, road.Count);
                road.SetTangentMode(range, TangentMode.AutoSmooth);
                road.SetAutoSmoothTension(range, 1 / 4f); // 1 is max curvy; 0 is min

                var roadLength = road.GetLength();

                // build road
                var roadEntity = ecb.CreateEntity();
                ecb.AddComponent(roadEntity, new Road
                {
                    Spline = new NativeSpline(road, Allocator.Persistent),
                    Length = roadLength,
                });

                // spawn cars on the road
                float splinePos = 0;
                int numVehicles = 0;
                while (splinePos < roadLength && numVehicles < trafficConfig.MaxVehiclesPerRoad && totalVehicles < trafficConfig.MaxVehiclesTotal)
                {
                    var prefab = vehiclePrefabs[random.NextInt(vehiclePrefabs.Length)].VehiclePrefab;
                    var vehicleEntity = ecb.Instantiate(prefab);

                    ecb.SetComponent(vehicleEntity, new VehiclePathing
                    {
                        SplinePos = splinePos,
                        RoadIndex = roadIndex,
                        RandomSeed = random.NextUInt()
                    });
                    
                    splinePos += random.NextFloat(trafficConfig.MinDistanceBetweenVehicles, trafficConfig.MaxDistanceBetweenVehicles);

                    numVehicles++;
                    totalVehicles++;
                }

                roadIndex++;
            }

            ecb.Playback(state.EntityManager);
            state.Enabled = false;

            Debug.Log($"total number of vehicles: {totalVehicles}");
        }
    }
}