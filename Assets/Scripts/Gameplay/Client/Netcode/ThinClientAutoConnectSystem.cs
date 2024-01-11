// using System.Linq;
// using Unity.Collections;
// using Unity.Entities;
// using Unity.MegacityMetro.Traffic;
// using Unity.MegacityMetro.UGS;
// using Unity.MegacityMetro.UI;
// using Unity.NetCode;
// using Unity.Networking.Transport;
// using UnityEngine;
//
// namespace Unity.MegacityMetro.Gameplay
// {
//     /// <summary>Quick utility class which caches the main client info, for easy lookup (for thin clients).</summary>
//     public static class MainClientCache
//     {
//         private static World s_MainClient;
//         private static EntityQuery s_DriverQuery;
//         private static EntityQuery s_ConnectionQuery;
//
//         /// <summary>Does the main client exist?</summary>
//         public static bool IsCreated => s_MainClient != null && s_MainClient.IsCreated;
//
//         /// <summary>Get connection info of the main client.</summary>
//         public static void GetMainClientInfo(out bool isCreated, out bool isConnected, out NetworkEndpoint localEndpoint, out NetworkEndpoint remoteEndpoint)
//         {
//             isCreated = false;
//             isConnected = default;
//             localEndpoint = default;
//             remoteEndpoint = default;
//
//             if (!IsCreated)
//             {
//                 foreach (var x in World.All)
//                 {
//                     if (x.IsClient() && !x.IsThinClient())
//                     {
//                         s_MainClient = x;
//                         s_DriverQuery = s_MainClient.EntityManager.CreateEntityQuery(typeof(NetworkStreamDriver));
//                         s_ConnectionQuery = s_MainClient.EntityManager.CreateEntityQuery(typeof(NetworkStreamConnection));
//                         goto found;
//                     }
//                 }
//                 return;
//             }
//
//             found:
//             isCreated = true;
//             isConnected = !s_DriverQuery.IsEmpty && !s_ConnectionQuery.IsEmpty;
//             if (!isConnected)
//                 return;
//
//             var mainClientDriver = s_DriverQuery.ToComponentDataArray<NetworkStreamDriver>(Allocator.Temp)[0];
//             var mainClientNsc = s_ConnectionQuery.ToComponentDataArray<NetworkStreamConnection>(Allocator.Temp)[0];
//             remoteEndpoint = mainClientDriver.GetRemoteEndPoint(mainClientNsc);
//             localEndpoint = mainClientDriver.GetLocalEndPoint();
//         }
//     }
//
//     /// <summary>
//     /// Auto-connects thin clients to the main client.
//     /// </summary>
//     [WorldSystemFilter(WorldSystemFilterFlags.ThinClientSimulation)]
//     public partial struct ThinClientAutoConnectSystem : ISystem
//     {
//         private EntityQuery m_NetworkStreamConnectionQuery;
//         private bool m_WasProperlyConnectedToServer;
//         private bool m_HasLoggedIntermediateStep;
//
//         public void OnCreate(ref SystemState state)
//         {
//             Debug.Log($"[{state.World.Name}] ThinClientAutoConnectSystem initialized.");
//             state.RequireForUpdate<NetworkStreamDriver>();
//         }
//
//         public void OnDestroy(ref SystemState state)
//         {
//             Debug.Log($"[{state.World.Name}] ThinClientAutoConnectSystem destroyed.");
//         }
//
//         public void OnUpdate(ref SystemState state)
//         {
//             // Only auto-connect thin clients once the main client is connecting, unless we should be automatically connecting.
//             var shouldAutoConnect = CommandLineConfig.AutomaticallyMatchmake && MatchMakingConnector.Instance.HasMatchmakingSuccess;
//             MainClientCache.GetMainClientInfo(out var mainClientIsCreated, out var mainClientIsConnected, out var mainClientLocalEp, out var mainClientRemoteEp);
//             var followClientIsInGame = MatchMakingConnector.Instance.ClientIsInGame || mainClientIsCreated;
//             // https://www.calculator.net/probability-calculator.html?cal4pa=0.0001&cal4par=3600&cal4pb=0.0001&cal4pbr=36000&calctype=series&x=44&y=21#series
//             // Probability of disconnect occuring within 1 minute = 1 - (1 - 0.0001)3600 = 30%
//             // Probability of disconnect occuring within 10 minutes = 1 - (1 - 0.0001)36000 = 97%
//             var randomDisconnectChance = UnityEngine.Random.value <= 0.0001f; 
//             var weAreConnectingOrConnected = SystemAPI.TryGetSingletonEntity<NetworkStreamConnection>(out var networkIdEntity);
//             var shouldWeBeInGame = (followClientIsInGame || shouldAutoConnect) && !randomDisconnectChance;
//
//             // Log to the console when we "properly" connect, for ease of debugging.
//             {
//                 var isProperlyConnectedToServer = SystemAPI.HasSingleton<NetworkId>();
//                 if (isProperlyConnectedToServer != m_WasProperlyConnectedToServer)
//                 {
//                     Debug.Log(isProperlyConnectedToServer 
//                         ? $"[{state.WorldUnmanaged.Name}] Successfully connected to server!" 
//                         : $"[{state.WorldUnmanaged.Name}] Disconnected from server!");
//                     m_WasProperlyConnectedToServer = isProperlyConnectedToServer;
//                 }
//             }
//
//             // Trying to match states...
//             if (weAreConnectingOrConnected == shouldWeBeInGame) 
//                 return;
//
//             if (weAreConnectingOrConnected)
//             {
//                 // Disconnect so that we match...
//                 // state.EntityManager.AddComponentData(networkIdEntity, new NetworkStreamRequestDisconnect { Reason = NetworkStreamDisconnectReason.ConnectionClose });
//                 // Debug.Log($"[{state.WorldUnmanaged.Name}] Auto-disconnecting Thin Client: followClientIsInGame: {followClientIsInGame}, shouldAutoConnect: {shouldAutoConnect}, randomDisconnectChance: {randomDisconnectChance}!");
//                 // m_HasLoggedIntermediateStep = false;
//             }
//             else
//             {
//                 // Connect so that we match...
//                 NetworkEndpoint targetEndpoint;
//                 if (CommandLineConfig.AutomaticallyMatchmake)
//                 {
//                     if (NetworkEndpoint.TryParse(MatchMakingConnector.Instance.IP, MatchMakingConnector.Instance.Port, out targetEndpoint)
//                         || NetworkEndpoint.TryParse(MatchMakingConnector.Instance.IP, MatchMakingConnector.Instance.Port, out targetEndpoint, NetworkFamily.Ipv6))
//                     {
//                         Debug.Log($"[{state.WorldUnmanaged.Name}] Auto-connecting Thin Client to address '{targetEndpoint}'!");
//                     }
//                     else
//                     {
//                         Debug.LogError($"[{state.WorldUnmanaged.Name}] Matchmaking endpoint is invalid! Cannot connect, cannot recover! Disposing this ThinClient world!");
//                         state.World.Dispose();
//                         return;
//                     }
//                 }
//                 else
//                 {
//                     // Find the main client, and connect to it's destination.
//                     if (!mainClientIsCreated && LogOnce())
//                     {
//                         Debug.Log($"[{state.WorldUnmanaged.Name}] Waiting for ThinClient to find main client to connect to...");
//                         return;
//                     }
//
//                     if (!mainClientIsConnected && LogOnce())
//                     {
//                         Debug.Log($"[{state.WorldUnmanaged.Name}] Waiting for 'Main Client' to connect, so we can follow it...");
//                         return;
//                     }
//
//                     Debug.Log($"[{state.WorldUnmanaged.Name}] Auto-connecting to main clients address '{mainClientRemoteEp}' (local: {mainClientLocalEp})!");
//                     targetEndpoint = mainClientRemoteEp;
//                 }
//
//                 ref var netStream = ref SystemAPI.GetSingletonRW<NetworkStreamDriver>().ValueRW;
//                 netStream.Connect(state.EntityManager, targetEndpoint);
//             }
//         }
//
//         bool LogOnce()
//         {
//             if (m_HasLoggedIntermediateStep)
//                 return false;
//             m_HasLoggedIntermediateStep = true;
//             return true;
//         }
//     }
// }
