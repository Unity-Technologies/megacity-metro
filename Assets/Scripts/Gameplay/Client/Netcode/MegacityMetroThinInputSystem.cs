using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

namespace Unity.MegacityMetro.Gameplay
{
    /// <summary>
    /// System to handle input for the thin client
    /// </summary>
    [UpdateInGroup(typeof(GhostInputSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ThinClientSimulation)]
    public partial struct AddThinClientInputSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkId>();
            state.RequireForUpdate<NetworkStreamInGame>();
            state.RequireForUpdate<NetworkTime>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingletonRW<CommandTarget>(out var commandTargetRw))
                return;

            // Ensure AI has input entity:
            if (commandTargetRw.ValueRO.targetEntity == Entity.Null || !state.EntityManager.HasComponent<PlayerVehicleInput>(commandTargetRw.ValueRO.targetEntity))
            {
                var inputEntity = state.EntityManager.CreateEntity();
                commandTargetRw.ValueRW.targetEntity = inputEntity;

                var connectionId = SystemAPI.GetSingleton<NetworkId>().Value;

                state.EntityManager.SetName(inputEntity, $"{nameof(AddThinClientInputSystem)}-RandInput");
                state.EntityManager.AddComponentData(inputEntity, new GhostOwner { NetworkId = connectionId });

                state.EntityManager.AddComponent<PlayerVehicleInput>(inputEntity);
                state.EntityManager.AddComponent<InputBufferData<PlayerVehicleInput>>(inputEntity);
            }
        }
    }

    [UpdateAfter(typeof(AddThinClientInputSystem))]
    [UpdateInGroup(typeof(GhostInputSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ThinClientSimulation)]
    public partial struct MoveThinClientInputSystem : ISystem
    {
        private NativeReference<Random> m_Rand;
        
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkId>();
            state.RequireForUpdate<NetworkStreamInGame>();
            state.RequireForUpdate<NetworkTime>();
            m_Rand = new NativeReference<Random>(Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            m_Rand.Value = Random.CreateFromIndex((uint) Stopwatch.GetTimestamp());
        }
        
        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            m_Rand.Dispose();
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingletonRW<CommandTarget>(out var commandTargetRw))
                return;
            
            // Recalculate AI action every x ticks:
            var networkTime = SystemAPI.GetSingleton<NetworkTime>();
            if (!networkTime.ServerTick.IsValid) 
                return;
            
            if (networkTime.ServerTick.TickIndexForValidTick % 30 == 0)
            {
                var input = state.EntityManager.GetComponentData<PlayerVehicleInput>(commandTargetRw.ValueRO.targetEntity);
                var rand = m_Rand.Value;

                // Movement:
                input.LookVelocity = (rand.NextFloat2() * 2) - new float2(1);

                // Drive:
                input.Acceleration = rand.NextInt() < 0.4f ? 0.5f : 0;

                // Roll:
                input.Roll = rand.NextFloat(-1f, 1f);

                // Shooting:
                input.Shoot = rand.NextInt() < 0.3f;

                m_Rand.Value = rand;
                state.EntityManager.SetComponentData(commandTargetRw.ValueRO.targetEntity, input);
            }
        }

        
    }
}
