using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.NetCode;
using Unity.NetCode.Extensions;
using Unity.Services.Samples.GameServerHosting;
using static Unity.Entities.SystemAPI;
using Random = Unity.Mathematics.Random;

namespace Unity.MegacityMetro.Gameplay
{
    /// <summary>
    /// System to set the player info.
    /// </summary>
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct SetPlayerInfoSystem : ISystem
    {
        #region Jobs

        [BurstCompile]
        private partial struct FindAvailableNameJob : IJob
        {
            public NativeArray<BotNameElement> Names;
            public NativeList<FixedString64Bytes> UsedNames;
            public Random Random;

            [BurstCompile]
            public void Execute()
            {
                var availablePlayerNames = GetAvailableNames();
                if (availablePlayerNames.Length == 0)
                {
                    UsedNames.Clear();
                    availablePlayerNames = GetAvailableNames();
                }

                // Choose a random player name from the list of available player names
                var randomIndex = Random.NextInt(0, availablePlayerNames.Length);
                var botName = availablePlayerNames[randomIndex];
                UsedNames.Add(botName);
                availablePlayerNames.Dispose();
            }

            private NativeList<FixedString64Bytes> GetAvailableNames()
            {
                var availablePlayerNames = new NativeList<FixedString64Bytes>(Allocator.TempJob);

                // Get a list of player names that have not been used
                foreach (var name in Names)
                {
                    if (!UsedNames.Contains(name.Name))
                    {
                        availablePlayerNames.Add(name.Name);
                    }
                }
                return availablePlayerNames;
            }
        }

        #endregion

        private NativeList<FixedString64Bytes> m_UsedNames;
        private Random m_Random;
        private EntityQuery m_ConnectedPlayers;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerConnectedElement>();
            state.RequireForUpdate<BotNameElement>();
            m_UsedNames = new NativeList<FixedString64Bytes>(Allocator.Persistent);
            var seed = DateTime.Now.Ticks / (DateTime.Now.Second + DateTime.Now.Day);
            m_Random = new Random((uint)seed);
            m_ConnectedPlayers = state.GetEntityQuery(ComponentType.ReadOnly<GhostOwner>());
            state.RequireForUpdate<ReceiveRpcCommandRequest>();
            state.RequireForUpdate<SetPlayerInfoRequest>();
        }

        //UGS - Removed Burst compile from here to allow static access to the MultiplayGameServerManager
        //TODO : Optimize for DOTS?
        public void OnUpdate(ref SystemState state)
        {
            var playersConnected = GetSingletonBuffer<PlayerConnectedElement>();
            var commandBuffer = GetSingletonRW<BeginSimulationEntityCommandBufferSystem.Singleton>().ValueRW.CreateCommandBuffer(state.WorldUnmanaged);
            var names = GetSingletonBuffer<BotNameElement>().ToNativeArray(Allocator.TempJob);

            foreach (var (info, request, requestEntity) in
                     Query<SetPlayerInfoRequest, ReceiveRpcCommandRequest>().WithEntityAccess())
            {
                var entityNetwork = request.SourceConnection;
                var networkId = GetComponent<NetworkId>(entityNetwork);

                foreach (var (ghostOwner, entity) in Query<RefRO<GhostOwner>>()
                             .WithEntityAccess())
                {
                    if (ghostOwner.ValueRO.NetworkId == networkId.Value)
                    {
                        var findANewNameJob = new FindAvailableNameJob
                        {
                            Names = names,
                            UsedNames = m_UsedNames,
                            Random = m_Random
                        };
                        state.Dependency = findANewNameJob.Schedule(state.Dependency);
                        state.Dependency.Complete();

                        // If the requester is not a client should use one from the bank of names.
                        var isNameEmpty = string.IsNullOrEmpty(info.Name.ToString());
                        var name = isNameEmpty ? BotNameGenerator.GetRandomName():info.Name;
                        name = info.IsClient ? name : m_UsedNames[m_UsedNames.Length - 1];
                        var uasID = info.UASId;

                        commandBuffer.SetComponent(entity, new PlayerName { Name = name });
                        commandBuffer.AddComponent(entity, new PlayerUASID { UASId = uasID });

                        playersConnected.Add(new PlayerConnectedElement { Name = name, UASId = uasID, Value = entity });
                        UnityEngine.Debug.Log($"Client: {name} ({uasID}) has joined the game!  (Thin = {!info.IsClient})\nConnected Players: {m_ConnectedPlayers.CalculateEntityCount()}");

                        GameHostingServerEvents.UserJoinedServer?.Invoke(uasID.ToString());
                        break;
                    }
                }

                commandBuffer.DestroyEntity(requestEntity);
            }
            names.Dispose();
        }
    }
}
