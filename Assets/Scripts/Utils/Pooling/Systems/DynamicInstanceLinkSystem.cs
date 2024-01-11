using Unity.Entities;
using Unity.Transforms;

namespace MegacityMetro.Pooling
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(TransformSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.LocalSimulation)]
    public partial struct DynamicInstanceLinkSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }
        
        public void OnDestroy(ref SystemState state)
        {
            foreach (var (dynamicInstanceLinkCleanup, entity) in 
                     SystemAPI.Query<RefRW<DynamicInstanceLinkCleanup>>()
                         .WithEntityAccess())
            {
                PoolManager.Instance.RecycleFromPool(dynamicInstanceLinkCleanup.ValueRO.InstanceIndex);
            }
        }

        public void OnUpdate(ref SystemState state)
        {
            var commandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.World.Unmanaged);
            
            // Spawn when entity created or enabled
            foreach (var (dynamicInstance, localToWorld, entity) in 
                     SystemAPI.Query<RefRW<DynamicInstanceLink>, RefRO<LocalToWorld>>()
                         .WithNone<DynamicInstanceLinkCleanup>()
                         .WithEntityAccess())
            {
                if (!dynamicInstance.ValueRO.Linked)
                {
                    var index = PoolManager.Instance.SpawnFromPool((PoolType)dynamicInstance.ValueRO.PoolType, localToWorld.ValueRO.Position);
                    dynamicInstance.ValueRW.Linked = true;

                    commandBuffer.AddComponent(entity, new DynamicInstanceLinkCleanup { InstanceIndex = index });
                }
            }
            
            // Despawn when entity destroyed
            foreach (var (dynamicInstanceLinkCleanup, entity) in 
                     SystemAPI.Query<RefRW<DynamicInstanceLinkCleanup>>()
                         .WithNone<DynamicInstanceLink>()
                         .WithEntityAccess())
            {
                PoolManager.Instance.RecycleFromPool(dynamicInstanceLinkCleanup.ValueRO.InstanceIndex);
                commandBuffer.RemoveComponent<DynamicInstanceLinkCleanup>(entity);
            }
            
            // Despawn when entity disabled
            foreach (var (dynamicInstanceLink, dynamicInstanceLinkCleanup, entity) in 
                     SystemAPI.Query<RefRW<DynamicInstanceLink>, RefRW<DynamicInstanceLinkCleanup>>()
                         .WithAll<Disabled>()
                         .WithOptions(EntityQueryOptions.IncludeDisabledEntities)
                         .WithEntityAccess())
            {
                PoolManager.Instance.RecycleFromPool(dynamicInstanceLinkCleanup.ValueRO.InstanceIndex);
                commandBuffer.RemoveComponent<DynamicInstanceLinkCleanup>(entity);
                dynamicInstanceLink.ValueRW.Linked = false;
            }
            
            // Transform update
            foreach (var (dynamicPoolLink, dynamicPoolLinkCleanup, localToWorld) in 
                     SystemAPI.Query<RefRO<DynamicInstanceLink>, RefRO<DynamicInstanceLinkCleanup>, RefRO<LocalToWorld>>())
            {
                if (!dynamicPoolLink.ValueRO.Linked)
                    continue;

                var index = dynamicPoolLinkCleanup.ValueRO.InstanceIndex;

                if (dynamicPoolLink.ValueRO.UpdatePosition)
                {
                    PoolManager.Instance.SetInstancePosition(index, localToWorld.ValueRO.Position);
                }

                if (dynamicPoolLink.ValueRO.UpdateRotation)
                {
                    PoolManager.Instance.SetInstanceRotation(index, localToWorld.ValueRO.Rotation);
                }
            }
        }
    }
}