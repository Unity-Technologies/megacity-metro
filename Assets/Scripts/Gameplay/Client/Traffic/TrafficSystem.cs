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
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.LocalSimulation)]
    public partial struct TrafficSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<VehiclePrefabRef>();
            state.RequireForUpdate<TrafficConfig>();
            state.RequireForUpdate<Road>();
            state.RequireForUpdate<TrafficEnabled>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var roadQuery = SystemAPI.QueryBuilder().WithAll<Road>().Build();
            var roads = roadQuery.ToComponentDataArray<Road>(state.WorldUpdateAllocator);
            var trafficConfig = SystemAPI.GetSingleton<TrafficConfig>();

            var job = new TrafficJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
                Speed = trafficConfig.GlobalCarSpeed,
                LaneOffsetScale = trafficConfig.LaneOffsetScale,
                BankingMax = trafficConfig.BankingMax,
                Roads = roads
            };
            job.ScheduleParallel();
        }

        [BurstCompile]
        public partial struct TrafficJob : IJobEntity
        {
            public float DeltaTime;
            public float Speed;
            public float LaneOffsetScale;
            public float BankingMax;
            [ReadOnly] public NativeArray<Road> Roads;

            public void Execute(ref LocalToWorld transform, ref VehiclePathing pathing)
            {
                var road = Roads[pathing.RoadIndex];
                pathing.SplinePos += Speed * DeltaTime;

                if (pathing.SplinePos > road.Length)
                {
                    pathing.SplinePos -= road.Length;
                }

                var random = new Random(pathing.RandomSeed);
                
                // random bank
                var up = math.up();
                var rotation = quaternion.RotateZ(random.NextFloat(-0.3f, 0.3f));
                up = math.rotate(rotation, up);
                
                // random offset from the path
                var randVec = random.NextFloat2(new float2(-1, -1), new float2(1, 1));
                randVec *= LaneOffsetScale;
                
                // to get a proper heading, we want priorPos on the spline, not where the car actually was, so we factor out the offset 
                var priorPos = transform.Position - (transform.Right * randVec.x) - (transform.Up * randVec.y);

                // for sake of world-relative speed, we compute the move in world units,
                // but then to get position on spline we need value from 0 to 1.0 
                var ratioAlongPath = pathing.SplinePos / road.Length;
                var newPos = road.Spline.EvaluatePosition(ratioAlongPath);
                var heading = math.normalize(newPos - priorPos);
                var rot = quaternion.LookRotation(heading, up);
                transform.Value = float4x4.TRS(newPos, rot, 1);
                
                // if we have non-1 scale, we would need to normalize Right and Up
                var offsetPos = transform.Position + (transform.Right * randVec.x) + (transform.Up * randVec.y);
                transform.Value.c3 = new float4(offsetPos, transform.Value.c3.w);
            }
        }
    }
}