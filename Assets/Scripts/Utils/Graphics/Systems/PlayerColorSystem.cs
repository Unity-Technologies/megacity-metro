using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.MegacityMetro.Utils;
using Unity.NetCode;
using Unity.Transforms;
using static Unity.Entities.SystemAPI;

namespace Unity.MegacityMetro.Gameplay
{
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct SubmeshColorSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MultiMaterialObjectTag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            // Add MaterialProperty component to all the submesh entities
            var commandBuffer = new EntityCommandBuffer(Allocator.TempJob);

            foreach (var (_, entity) in Query<RefRO<MultiMaterialObjectTag>>().WithEntityAccess()
                         .WithNone<MultiMaterialUpdated>())
            {
                if (HasBuffer<Child>(entity))
                {
                    var child = GetBuffer<Child>(entity);
                    for (var i = 0; i < child.Length; i++)
                    {
                        var c = child[i].Value;
                        // Only add the component if it hasn't been added yet
                        if (!HasComponent<LayeredLitBaseColor3>(c))
                        {
                            commandBuffer.AddComponent<LayeredLitBaseColor3>(c);
                        }
                    }

                    // Add component so we can cleanup this later
                    commandBuffer.AddComponent<MultiMaterialUpdated>(entity);
                }
            }

            commandBuffer.Playback(state.EntityManager);
            commandBuffer.Dispose();
        }
    }

    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct PlayerColorSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MultiMaterialUpdated>();
            state.RequireForUpdate<NetworkId>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var commandBuffer = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (ghostOwner, linkedEntityGroup) in Query<RefRO<GhostOwner>, DynamicBuffer<LinkedEntityGroup>>()
                         .WithAll<SetPlayerColorTag>())
            {
                var deterministicColor = PlayerColor.Get(ghostOwner.ValueRO.NetworkId);
                foreach (var linkedEntity in linkedEntityGroup)
                {
                    if (HasComponent<LayeredLitBaseColor3>(linkedEntity.Value))
                    {
                        commandBuffer.SetComponent(linkedEntity.Value, new LayeredLitBaseColor3 {Value = deterministicColor});
                    }
                }

                var ghostEntity = linkedEntityGroup[0];
                commandBuffer.RemoveComponent<SetPlayerColorTag>(ghostEntity.Value);
            }

            // Cleanup MultiMaterialObject so we don't run Systems that we don't need
            foreach (var (_, entity) in Query<RefRO<MultiMaterialObjectTag>>().WithEntityAccess()
                         .WithAll<MultiMaterialUpdated>())
            {
                commandBuffer.RemoveComponent<MultiMaterialObjectTag>(entity);
                commandBuffer.RemoveComponent<MultiMaterialUpdated>(entity);
            }

            commandBuffer.Playback(state.EntityManager);
            commandBuffer.Dispose();
        }
    }
}
