using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.NetCode.Extensions;
using Unity.Physics;
using Unity.Transforms;
using static Unity.Entities.SystemAPI;

namespace Unity.MegacityMetro.Gameplay
{
    /// <summary>
    /// System to update the player name tags.
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct UpdatePlayerNameTag : ISystem, ISystemStartStop
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PhysicsWorldSingleton>();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (PlayerInfoController.Instance == null)
                return;

            var commandBuffer = new EntityCommandBuffer(Allocator.TempJob);

            foreach (var (playerName, health, entity) in Query<RefRO<PlayerName>, RefRO<VehicleHealth>>()
                         .WithNone<PlayerNameTag, GhostOwnerIsLocal>()
                         .WithEntityAccess())
            {
                var name = playerName.ValueRO.Name.ToString();
                var currentHealth = health.ValueRO.Value;
                PlayerInfoController.Instance.CreateNameTag(name, entity, currentHealth);
                commandBuffer.AddComponent<PlayerNameTag>(entity);
            }

            foreach (var (localToWorld, health, player, entity) in Query<RefRO<LocalToWorld>, RefRO<VehicleHealth>,
                         RefRO<PlayerName>>().WithEntityAccess())
            {
                var healthValue = health.ValueRO.Value;
                var playerName = player.ValueRO.Name.ToString();
                PlayerInfoController.Instance.UpdateNamePosition(entity, playerName, healthValue, localToWorld.ValueRO);
            }

            PlayerInfoController.Instance.RefreshNameTags(state.EntityManager);
            commandBuffer.Playback(state.EntityManager);
            commandBuffer.Dispose();
        }

        public void OnStartRunning(ref SystemState state)
        {
        }

        public void OnStopRunning(ref SystemState state)
        {
            if (PlayerInfoController.Instance != null)
                PlayerInfoController.Instance.ClearNames();
        }
    }
}