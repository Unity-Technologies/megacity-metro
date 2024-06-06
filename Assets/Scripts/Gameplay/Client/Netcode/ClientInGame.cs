using Unity.Collections;
using Unity.Entities;
using Unity.MegacityMetro.Gameplay;
using Unity.NetCode;
using Unity.Transforms;

namespace Unity.MegacityMetro
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct ClientInGame : ISystem
    {
        private bool m_HasRegisteredSmoothingAction;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerSpawner>();

            // Client-side optimizations:
            var ghostSendSystemData = new GhostSendSystemData {MinSendImportance = 2};
            state.EntityManager.CreateSingleton(ghostSendSystemData);
            
            RateSettings.ApplyFrameRate();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (!m_HasRegisteredSmoothingAction &&
                SystemAPI.TryGetSingletonRW<GhostPredictionSmoothing>(out var ghostPredictionSmoothing))
            {
                m_HasRegisteredSmoothingAction = true;
                ghostPredictionSmoothing.ValueRW.RegisterSmoothingAction<LocalTransform>(state.EntityManager,
                    MegacityMetroSmoothingAction.Action);
            }

            var cmdBuffer = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (_, entity) in SystemAPI.Query<RefRO<NetworkId>>().WithNone<NetworkStreamInGame>()
                         .WithEntityAccess())
            {
                cmdBuffer.AddComponent<NetworkStreamInGame>(entity);
                cmdBuffer.AddComponent<ConnectionState>(entity);
            }

            cmdBuffer.Playback(state.EntityManager);
        }
    }
}