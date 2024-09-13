using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.NetCode.Extensions;
using static Unity.Entities.SystemAPI;

namespace Unity.MegacityMetro.Gameplay
{
    /// <summary>
    /// Component added to the player entity when the Vivox connection is established.
    /// </summary>
    public struct VivoxConnectionStatus : IComponentData
    {
    }

    /// <summary>
    /// System that initializes the Vivox connection for the local player.
    /// </summary>
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct InitializedVivoxSystem : ISystem
    {
        private EntityQuery m_PlayerQuery;

        public void OnCreate(ref SystemState state)
        {
#if UNITY_SERVER
            state.Enabled = false;
            return;
#endif
            state.RequireForUpdate<NetworkStreamInGame>();
            m_PlayerQuery = state.GetEntityQuery(ComponentType.ReadOnly<GhostOwnerIsLocal>(),
                ComponentType.Exclude<VivoxConnectionStatus>());
            state.RequireForUpdate(m_PlayerQuery);
        }

        public void OnUpdate(ref SystemState state)
        {
            if (PlayerInfoController.Instance == null || 
                PlayerInfoController.Instance.IsSinglePlayer || 
                VivoxManager.Instance == null || 
                VivoxManager.Instance.Service == null)
                return;

            var cmdBuffer = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (playerName, entity) in Query<RefRO<PlayerName>>()
                         .WithAll<GhostOwnerIsLocal>()
                         .WithNone<VivoxConnectionStatus>()
                         .WithEntityAccess())
            {
                if (!string.IsNullOrEmpty(playerName.ValueRO.Name.ToString()))
                {
                    VivoxManager.Instance.Session.Login(playerName.ValueRO.Name.ToString());
                    cmdBuffer.AddComponent<VivoxConnectionStatus>(entity);
                }
            }

            cmdBuffer.Playback(state.EntityManager);
            cmdBuffer.Dispose();
        }
    }
}