using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Unity.MegacityMetro.Traffic
{
    //     Author the traffic sections and merges
    [TemporaryBakingType]
    public struct TrafficPathBakingData : IComponentData
    {
        public UnityObjectRef<Path> Authoring;
        public Entity Entity;
    }

    [BakingVersion("megacity-metro", 4)]
    public class TrafficPathBaker : Baker<Path>
    {
        public override void Bake(Path authoring)
        {
            var entity = GetEntity(authoring.gameObject, TransformUsageFlags.Dynamic);
            AddComponent(entity, new TrafficPathBakingData { Authoring = authoring, Entity = entity });

            // for new traffic system
            var roadPoints = AddBuffer<RoadPoint>(entity);
            roadPoints.Length = authoring.Waypoints.Count;

            for (int i = 0; i < authoring.Waypoints.Count; i++)
            {
                roadPoints[i] = new RoadPoint { Value = authoring.Waypoints[i].position };
            }

            // (we need the world coord when we spawn the roads at runtime)
            AddComponent(entity, new RoadOffset { Value = authoring.transform.position });
        }
    }

    [WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
    [UpdateInGroup(typeof(PostBakingSystemGroup))]
    [BakingVersion("megacity-metro", 1)]
    internal partial class TrafficPathBakingSystem : SystemBase
    {
        public Dictionary<float3, int> MergeMap = new();

        // TODO: use native hashmap
        public Dictionary<int, int> PathMap = new();
        public Dictionary<float3, int> RampMap = new();
        public int RIndex;

        protected override void OnUpdate()
        {
            var sections = new NativeList<RoadSection>(Allocator.Temp);
            var rootSceneEntity = Entity.Null;
            Entities.ForEach((Entity e, ref TrafficPathBakingData trafficPathBakingData) =>
            {
                var path = trafficPathBakingData.Authoring.Value;
                if (rootSceneEntity == Entity.Null)
                {
                    //The first entity of the scene is used to store the RoadSectionBlob [Blob Asset]
                    rootSceneEntity = e;
                }

                var entity = trafficPathBakingData.Entity;
                var numPathNodes = path.GetNumNodes();
                PathMap.Add(path.GetInstanceID(), RIndex);

                var roadSettings = path.gameObject.GetComponentInParent<RoadSettings>();

                uint spawnPool = 0;
                if (roadSettings != null)
                {
                    spawnPool = roadSettings.vehicleSelection;
                }

                for (var n = 1; n < numPathNodes; n++)
                {
                    var rs = new RoadSection();

                    path.GetSplineSection(n - 1, out rs.p0, out rs.p1, out rs.p2, out rs.p3);
                    rs.arcLength = CatmullRom.ComputeArcLength(rs.p0, rs.p1, rs.p2, rs.p3, 1024);

                    rs.vehicleHalfLen = Constants.VehicleLength / rs.arcLength;
                    rs.vehicleHalfLen /= 2;

                    rs.sortIndex = RIndex;
                    rs.linkNext = RIndex + 1;
                    if (n == numPathNodes - 1)
                    {
                        rs.linkNext = -1;
                        MergeMap[
                            math.round((path.GetReversibleRawPosition(n) + new float3(path.transform.position)) *
                                       Constants.NodePositionRounding) / Constants.NodePositionRounding] = RIndex;
                    }

                    rs.linkExtraChance = 0.0f;
                    rs.linkExtra = -1;

                    rs.width = path.width;
                    rs.height = path.height;

                    rs.minSpeed = path.minSpeed;
                    rs.maxSpeed = path.maxSpeed;

                    rs.occupationLimit = math.min(Constants.RoadOccupationSlotsMax,
                        (int)math.round(rs.arcLength / Constants.VehicleLength));

                    if (!path.isOnRamp)
                    {
                        RampMap[
                            math.round((path.GetReversibleRawPosition(n - 1) + new float3(path.transform.position)) *
                                       Constants.NodePositionRounding) / Constants.NodePositionRounding] = RIndex;
                        if (n == 1)
                        {
                            var x = 2; // Only spawn in right lane
                            var t = rs.vehicleHalfLen;
                            var pathTime = n - 1 + t;

                            var spawner = new Spawner();
                            spawner.Time = math.frac(pathTime);

                            spawner.Direction = math.normalize(path.GetTangent(pathTime));

                            float3 rightPos = (x - 1) * math.mul(spawner.Direction, Vector3.right) *
                                              ((rs.width - Constants.VehicleWidth) / 2.0f);
                            spawner.Position = path.GetWorldPosition(pathTime) + rightPos;

                            spawner.RoadIndex = RIndex;

                            spawner.minSpeed = path.minSpeed;
                            spawner.maxSpeed = path.maxSpeed;

                            var speedInverse = 1.0f / spawner.minSpeed;

                            spawner.random = new Random((uint)RIndex + 1);
                            spawner.delaySpawn = (int)Constants.VehicleLength + spawner.random.NextInt(
                                (int)(speedInverse * 60.0f),
                                (int)(speedInverse * 120.0f));

                            spawner.LaneIndex = x;
                            spawner.poolSpawn = spawnPool;

                            // Each path will only have one spawner, so use the Primary Entity
                            EntityManager.AddComponentData(entity, spawner);
                        }
                    }

                    RIndex++;
                    sections.Add(rs);
                }
            }).WithStructuralChanges().Run();
            if (rootSceneEntity == Entity.Null)
            {
                return;
            }

            // Loop over the paths again to start building the ramps and merges
            Entities.ForEach((ref TrafficPathBakingData trafficPathBakingData) =>
            {
                var path = trafficPathBakingData.Authoring.Value;
                var numPathNodes = path.GetNumNodes();

                // Handle On Ramp roads
                if (path.isOnRamp)
                {
                    var rsRampIdx = PathMap[path.GetInstanceID()];
                    var rampEntry =
                        math.round((path.GetReversibleRawPosition(0) + new float3(path.transform.position)) *
                                   Constants.NodePositionRounding) / Constants.NodePositionRounding;
                    if (RampMap.ContainsKey(rampEntry))
                    {
                        var rsIndex = RampMap[rampEntry];
                        if (rsIndex > 0)
                        {
                            rsIndex -= 1;
                            if (sections.Length > rsIndex)
                            {
                                var rs = sections[rsIndex];
                                if (rs.linkNext == rsIndex + 1)
                                {
                                    rs.linkExtra = rsRampIdx;
                                    rs.linkExtraChance = path.percentageChanceForOnRamp / 100.0f;
                                    sections[rsIndex] = rs;
                                }
                            }
                        }
                    }
                }

                // Handle merging roads
                {
                    var n = 0;

                    var pos =
                        math.round((path.GetReversibleRawPosition(n) + new float3(path.transform.position)) *
                                   Constants.NodePositionRounding) / Constants.NodePositionRounding;
                    if (MergeMap.ContainsKey(pos))
                    {
                        var mergeFromIndex = MergeMap[pos];
                        var mergeToBase = PathMap[path.GetInstanceID()];

                        if (mergeFromIndex > 0 && mergeFromIndex != mergeToBase + n)
                        {
                            var rs = sections[mergeFromIndex];
                            if (rs.linkNext == -1)
                            {
                                rs.linkNext = mergeToBase + n;
                                sections[mergeFromIndex] = rs;
                            }
                        }
                    }
                }
            }).WithoutBurst().Run();
            using var blobBuilder = new BlobBuilder(Allocator.Temp);
            ref var sectionsBlob = ref blobBuilder.ConstructRoot<RoadSectionBlob>();
            var sectionsDataBuilder = blobBuilder.Allocate(ref sectionsBlob.RoadSections, sections.Length);
            for (var i = 0; i < sections.Length; i++)
            {
                sectionsDataBuilder[i] = sections[i];
            }

            var blobRef = blobBuilder.CreateBlobAssetReference<RoadSectionBlob>(Allocator.Persistent);
            EntityManager.AddComponentData(rootSceneEntity,
                new RoadSectionBlobRef
                {
                    Data = blobRef
                });
            RIndex = 0;
            PathMap.Clear();
            RampMap.Clear();
            MergeMap.Clear();
        }
    }
}
