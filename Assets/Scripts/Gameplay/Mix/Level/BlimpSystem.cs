using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Unity.MegacityMetro.Gameplay
{
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.LocalSimulation)]
    public partial struct BlimpSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (target, transform) in SystemAPI.Query<RefRW<BlimpComponent>, RefRW<LocalTransform>>())
            {
                // angle value for circle
                target.ValueRW.Rotation -= target.ValueRW.RotationSpeed * SystemAPI.Time.DeltaTime;
                
                if (target.ValueRW.Rotation < -360.0f)
                {
                    target.ValueRW.Rotation = 0.0f;
                }

                // position on circle based on random radius
                var x = target.ValueRO.StartingPosition.x + target.ValueRO.TravelRadius *
                    math.cos(target.ValueRO.Rotation + target.ValueRO.RandomStartPoint);
                var z = target.ValueRO.StartingPosition.z + target.ValueRO.TravelRadius *
                    math.sin(target.ValueRO.Rotation + target.ValueRO.RandomStartPoint);

                // position ahead on the circle
                var x2 = target.ValueRO.StartingPosition.x + target.ValueRO.TravelRadius *
                    math.cos(target.ValueRO.Rotation + target.ValueRO.RandomStartPoint + target.ValueRO.LookAheadValue);
                var z2 = target.ValueRO.StartingPosition.z + target.ValueRO.TravelRadius *
                    math.sin(target.ValueRO.Rotation + target.ValueRO.RandomStartPoint + target.ValueRO.LookAheadValue);

                var pointOnCircle = new float3(x, target.ValueRO.StartingPosition.y, z);
                var pointOnCircle2 = new float3(x2, target.ValueRO.StartingPosition.y, z2);
                var lookVector = pointOnCircle - pointOnCircle2;

                transform.ValueRW.Position = pointOnCircle;
                transform.ValueRW.Rotation = quaternion.LookRotation(lookVector, math.up());
            }
        }
    }
}