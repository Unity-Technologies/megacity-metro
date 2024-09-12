using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.MegacityMetro.Gameplay;
using Unity.NetCode;
using Unity.NetCode.Extensions;
using Unity.Transforms;
using UnityEngine;
#if UNITY_SERVER && !UNITY_EDITOR
using Unity.Networking.Transport;
#endif

namespace Unity.MegacityMetro
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderLast = true)]
    [CreateAfter(typeof(NetworkStreamReceiveSystem))]
    public partial struct ServerInGame : ISystem
    {
        #region Jobs

        [BurstCompile]
        private partial struct GetPositionJob : IJob
        {
            public NativeArray<SpawnPointElement> SpawnPoints;
            public NativeList<float3> UsedPositions;
            public Mathematics.Random Random;

            [BurstCompile]
            public void Execute()
            {
                var availablePositions = CreateAvailablePositions();

                // If all player positions have been used, reset the list
                if (availablePositions.Length == 0)
                {
                    UsedPositions.Clear();
                    availablePositions = CreateAvailablePositions();
                }

                // Choose a random position from the list of available player names
                var randomIndex = Random.NextInt(0, availablePositions.Length);
                var position = availablePositions[randomIndex];
                UsedPositions.Add(position);
                availablePositions.Dispose();
            }

            private NativeList<float3> CreateAvailablePositions()
            {
                var availablePositions = new NativeList<float3>(Allocator.TempJob);

                // Get a list of spawnPoints that have not been used
                foreach (var position in SpawnPoints)
                {
                    if (!UsedPositions.Contains(position.Value))
                    {
                        availablePositions.Add(position.Value);
                    }
                }

                return availablePositions;
            }
        }

        [BurstCompile]
        partial struct UpdateConnectionPositionSystemJob : IJobEntity
        {
            [ReadOnly] public ComponentLookup<LocalTransform> transformLookup;

            public void Execute(ref GhostConnectionPosition conPos, in CommandTarget target)
            {
                if (!transformLookup.HasComponent(target.targetEntity))
                    return;
                conPos = new GhostConnectionPosition
                {
                    Position = transformLookup[target.targetEntity].Position
                };
            }
        }

        #endregion

        private NativeList<float3> m_UsedPositions;
        private Mathematics.Random m_Random;

        public void OnCreate(ref SystemState state)
        {
            var myEntity = state.EntityManager.CreateEntity();
            state.EntityManager.AddBuffer<PlayerConnectedElement>(myEntity);
            state.RequireForUpdate<PlayerConnectedElement>();
            
            state.RequireForUpdate<PlayerSpawner>();
            state.RequireForUpdate<SpawnPointElement>();
            m_UsedPositions = new NativeList<float3>(Allocator.Persistent);
            var currentTime = DateTime.Now;
            var seed = currentTime.Minute + currentTime.Second + currentTime.Millisecond + 1;
            m_Random = new Mathematics.Random((uint)seed);
#if UNITY_SERVER && !UNITY_EDITOR
            SystemAPI.GetSingletonRW<NetworkStreamDriver>().ValueRW.Listen(NetworkEndpoint.AnyIpv4.WithPort(CommandLineConfig.MultiplayEndpoint.Port));
#endif
            const int tileSize = 256;
            var grid = state.EntityManager.CreateEntity();
            state.EntityManager.SetName(grid, "GhostImportanceSingleton");
            state.EntityManager.AddComponentData(grid, new GhostDistanceData
            {
                TileSize = new int3(tileSize, 1024 * 8, tileSize),
                TileCenter = new int3(0, 0, 0),
                TileBorderWidth = new float3(5f),
            });
            state.EntityManager.AddComponentData(grid, new GhostImportance
            {
                ScaleImportanceFunction = GhostDistanceImportance.ScaleFunctionPointer,
                GhostConnectionComponentType = ComponentType.ReadOnly<GhostConnectionPosition>(),
                GhostImportanceDataType = ComponentType.ReadOnly<GhostDistanceData>(),
                GhostImportancePerChunkDataType = ComponentType.ReadOnly<GhostDistancePartitionShared>(),
            });
            
            var clientServerTickRate = new ClientServerTickRate();
            clientServerTickRate.ResolveDefaults();
            clientServerTickRate.SimulationTickRate = clientServerTickRate.NetworkTickRate = RateSettings.tickRate;
            clientServerTickRate.PredictedFixedStepSimulationTickRatio = RateSettings.fixedRateRatio;
            state.EntityManager.CreateSingleton(clientServerTickRate);
            
            RateSettings.ApplyFrameRate();
        }

        public void OnUpdate(ref SystemState state)
        {
            var spawnBuffer = SystemAPI.GetSingletonBuffer<SpawnPointElement>();
            var prefab = SystemAPI.GetSingleton<PlayerSpawner>().Player;
            var cmdBuffer = SystemAPI.GetSingletonRW<BeginSimulationEntityCommandBufferSystem.Singleton>().ValueRW.CreateCommandBuffer(state.WorldUnmanaged);
            var originalTrans = state.EntityManager.GetComponentData<LocalTransform>(prefab);
            var health = state.EntityManager.GetComponentData<VehicleHealth>(prefab);

            foreach (var (netId, entity) in SystemAPI.Query<RefRO<NetworkId>>().WithNone<NetworkStreamInGame>()
                         .WithEntityAccess())
            {
                var playerBuffer = SystemAPI.GetSingletonBuffer<PlayerConnectedElement>(true);
                var networkIdIsBusy = false;
                foreach (var playerElement in playerBuffer)
                {
                    if (playerElement.NetworkID == netId.ValueRO.Value)
                    {
                        networkIdIsBusy = true;
                        break;
                    }
                }

                if (networkIdIsBusy)
                {
                    Debug.LogWarning($"The NetworkID {netId.ValueRO.Value} is busy, the player couldn't be created");
                    continue;    
                }
                
                var spawnPointsArray = spawnBuffer.ToNativeArray(Allocator.TempJob);
                var findNewPosition = new GetPositionJob
                {
                    SpawnPoints = spawnPointsArray,
                    UsedPositions = m_UsedPositions,
                    Random = m_Random
                };
                state.Dependency = findNewPosition.Schedule(state.Dependency);
                state.Dependency.Complete();

                cmdBuffer.AddComponent<NetworkStreamInGame>(entity);
                var player = cmdBuffer.Instantiate(prefab);
                var networkIdValue = netId.ValueRO.Value;
                cmdBuffer.SetComponent(player, new GhostOwner { NetworkId = networkIdValue });
                var newTrans = originalTrans;
                newTrans.Position = m_UsedPositions[m_UsedPositions.Length - 1];

                cmdBuffer.SetComponent(player, newTrans);
                cmdBuffer.AppendToBuffer(entity, new LinkedEntityGroup { Value = player });
                cmdBuffer.SetComponent(player, health);

                cmdBuffer.AddComponent<GhostConnectionPosition>(entity);
                cmdBuffer.SetComponent(entity, new CommandTarget { targetEntity = player });
                
                spawnPointsArray.Dispose();
            }

            var updateJob = new UpdateConnectionPositionSystemJob
            {
                transformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true)
            };

            state.Dependency = updateJob.ScheduleParallel(state.Dependency);
        }
    }
}