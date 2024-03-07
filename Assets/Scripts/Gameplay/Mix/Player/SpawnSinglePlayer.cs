using Unity.Collections;
using Unity.Entities;
using Unity.MegacityMetro.CameraManagement;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

namespace Unity.MegacityMetro.Gameplay
{
    
    /// <summary>
    /// System to spawn the player.
    /// </summary>
    public partial struct SpawnSinglePlayer : ISystem
    {
        private EntityQuery m_PlayerQuery;

        public void OnCreate(ref SystemState state)
        {
            m_PlayerQuery = state.GetEntityQuery(ComponentType.ReadOnly<SinglePlayer>());
            state.RequireForUpdate<PlayerSpawner>();
            state.RequireForUpdate<SpawnPointElement>();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (PlayerInfoController.Instance != null && !PlayerInfoController.Instance.IsSinglePlayer || m_PlayerQuery.CalculateEntityCount() > 0)
                return;
            
            var spawnBuffer = SystemAPI.GetSingletonBuffer<SpawnPointElement>();
            var cmdBuffer = new EntityCommandBuffer(Allocator.Temp);
            var prefab = SystemAPI.GetSingleton<PlayerSpawner>().SinglePlayer;
            var player = cmdBuffer.Instantiate(prefab);
            var originalTrans = state.EntityManager.GetComponentData<LocalTransform>(prefab);
            var newTrans = originalTrans;
            newTrans.Position = spawnBuffer.ElementAt(0).Value;
            cmdBuffer.SetComponent(player, newTrans);
            cmdBuffer.AddComponent<SinglePlayer>(player);
            cmdBuffer.AddComponent<GhostOwnerIsLocal>(player);
            
            cmdBuffer.Playback(state.EntityManager);
        }
    }
}