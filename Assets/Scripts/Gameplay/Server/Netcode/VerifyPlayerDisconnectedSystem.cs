using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using Unity.NetCode.Extensions;
using Unity.Services.Samples.GameServerHosting;
using static Unity.Entities.SystemAPI;

namespace Unity.MegacityMetro.Gameplay
{
    /// <summary>
    /// Verify if a player has been disconnected from the game.
    /// </summary>
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct VerifyPlayerDisconnectedSystem : ISystem
    {
        private EntityQuery m_ConnectedPlayers;
        public void OnCreate(ref SystemState state)
        {
            m_ConnectedPlayers = state.GetEntityQuery(ComponentType.ReadOnly<NetworkStreamConnection>());
        }

        public void OnUpdate(ref SystemState state)
        {
            var playersConnected = GetSingletonBuffer<PlayerConnectedElement>();

            for (int i = 0; i < playersConnected.Length; i++)
            {
                var player = playersConnected[i];
                if (!state.EntityManager.Exists(player.Value))
                {
                    var uasID = player.UASId.ToString();
                    GameHostingServerEvents.UserLeftServer?.Invoke(uasID);

                    UnityEngine.Debug.Log($"Attention: Player {player.Name}({uasID}) has been disconnected from the game." +
                                          $"\nConnected Players: {m_ConnectedPlayers.CalculateEntityCount()}");
                    playersConnected.RemoveAt(i);
                }
            }
        }
    }
}
