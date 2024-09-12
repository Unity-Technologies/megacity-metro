using Unity.Entities;
using Unity.NetCode;
using Unity.NetCode.Extensions;
using Unity.Transforms;
using static Unity.Entities.SystemAPI;

namespace Unity.MegacityMetro.Gameplay
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct InitializePlayerNameTag : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (PlayerInfoController.Instance == null)
                return;
            
            var ecb = GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            // Create name tags for players that don't have one
            foreach (var (playerName, health, entity) in Query<RefRO<PlayerName>, RefRO<VehicleHealth>>()
                         .WithNone<PlayerNameTag, GhostOwnerIsLocal>()
                         .WithEntityAccess())
            {
                var name = playerName.ValueRO.Name.ToString();
                var currentHealth = health.ValueRO.Value;
                PlayerInfoController.Instance.CreateNameTag(name, entity, currentHealth);
                ecb.AddComponent<PlayerNameTag>(entity);
            }
        }
    }

    /// <summary>
    /// System to update the player name tags.
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    [UpdateAfter(typeof(InitializePlayerNameTag))]
    public partial struct UpdatePlayerNameTag : ISystem, ISystemStartStop
    {
        public void OnUpdate(ref SystemState state)
        {
            if (PlayerInfoController.Instance == null)
                return;

            // Update name tags for players that have one
            foreach (var (localToWorld, health, player, entity) in Query<RefRO<LocalToWorld>, RefRO<VehicleHealth>,
                         RefRO<PlayerName>>().WithEntityAccess())
            {
                var healthValue = health.ValueRO.Value;
                var playerName = player.ValueRO.Name;
                PlayerInfoController.Instance.UpdateNamePosition(entity, playerName, healthValue, localToWorld.ValueRO);
            }

            PlayerInfoController.Instance.RefreshNameTags(state.EntityManager);
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