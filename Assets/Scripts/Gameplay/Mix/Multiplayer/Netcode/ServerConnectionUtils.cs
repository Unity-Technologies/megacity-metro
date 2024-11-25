using System;
using System.Net;
using System.Text.RegularExpressions;
using Unity.NetCode;
using Unity.Networking.Transport;
using UnityEngine;
using Unity.Entities;

namespace Unity.MegacityMetro
{
    public class ServerConnectionUtils
    {
        private static bool IsTryingToConnect;

        public static bool TryParseNetworkEntrypoint(string address, ushort port, out NetworkEndpoint endpoint)
        {

            return NetworkEndpoint.TryParse(address, port, out endpoint, NetworkFamily.Ipv4) ||
                   NetworkEndpoint.TryParse(address, port, out endpoint, NetworkFamily.Ipv6);
        }

        public static bool TryParseDomain(string domainName, out string IP)
        {
            IP = string.Empty;
            if (IsValidUrl(domainName))
            {
                domainName = domainName.Replace("https://", "").Replace("http://", "");
                try
                {
                    IPAddress[] addresses = Dns.GetHostAddresses(domainName);

                    if (addresses.Length > 0)
                    {
                        foreach (IPAddress address in addresses)
                        {
                            if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                IP = address.ToString();
                                return true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"The domain is no valid {domainName}:\n{ex.Message}");
                }
            }

            return false;
        }

        private static bool IsValidUrl(string url)
        {
            var ip = url;
            if (url.Contains(":"))
            {
                var ipElements = ip.Split(":");
                ip = ipElements[0];
            }

            // add an early out if the IP is valid
            if (IPAddress.TryParse(ip, out var ipAddress))
            {
                Debug.Log($"IP Address {ipAddress} is valid but it's not an URL");
                return false; 
            }
            
            var urlPattern = @"^(https?|ftp):\/\/[^\s\/$.?#].[^\s]*$";
            if (!Regex.IsMatch(url, urlPattern, RegexOptions.IgnoreCase))
            {
                Debug.LogError($"it is not a valid url {url}");
                return false;
            }

            return true;
        }
        
        public static void RequestConnection(string ip, ushort port)
        {
            if (IsTryingToConnect)
            {
                Debug.LogError($"RequestConnection already called! Ignoring request to {ip} : {port}!");
                return;
            }

            if(!TryParseNetworkEntrypoint(ip, port, out var targetEndpoint))
            {
                Debug.LogError($"Failed to parse IP '{ip}' as valid address! Cannot connect to '{ip}:{port}'!");
                return; 
            }

            if (CommandLineConfig.AutomaticallyMatchmake && CommandLineConfig.AutomaticallyAddThinClients && Application.isBatchMode)
            {
                Debug.Log("Not creating a ClientWorld or ServerWorld, as we're using the automatic flow for thin clients.");
                return;
            }

            if (ClientServerBootstrap.RequestedPlayType == ClientServerBootstrap.PlayType.ClientAndServer)
            {
                var targetEndpointIsValidForClientAndServer = (targetEndpoint.IsAny || targetEndpoint.IsLoopback);
                if (targetEndpointIsValidForClientAndServer)
                {
                    Debug.Log($"StartClientServer: {ip} : {port}!");
                    StartClientServer(targetEndpoint);
                    return;
                }
                Debug.LogWarning($"Cannot adhere to RequestedPlayType.ClientAndServer with targetEndpoint '{targetEndpoint}'! Creating a ClientWorld only!");
            }
            else if (ClientServerBootstrap.RequestedPlayType == ClientServerBootstrap.PlayType.Server)
            {
                var targetEndpointIsValidForServer = (targetEndpoint.IsAny || targetEndpoint.IsLoopback);
                if (!targetEndpointIsValidForServer)
                    Debug.LogWarning($"Ignoring address argument for RequestedPlayType.Server with targetEndpoint '{targetEndpoint}' as it's not a valid server address!");
                Debug.Log($"StartServer: {ip} : {port}!");
                StartServer(targetEndpoint);
                return;
                
                
            }

            Debug.Log($"ConnectToServer: {ip} : {port}!");
            ConnectToServer(targetEndpoint);
        }

        public static void CreateDefaultWorld(bool isLocal = false)
        {
            DestroyActiveSimulationWorld();
            ClientServerBootstrap.AutoConnectPort = 0;
            World newWorld;

            if (isLocal)
            {
                newWorld = ClientServerBootstrap.CreateLocalWorld("LocalWorld");
            }
            else
            {
                newWorld = ClientServerBootstrap.CreateClientWorld("ClientWorld");
            }

            World.DefaultGameObjectInjectionWorld ??= newWorld;
        }

        
        /// <summary>
        ///     Start a Client and Server in-proc, and connect them together.
        /// </summary>
        private static void StartClientServer(NetworkEndpoint targetEndpoint)
        {
            if(!targetEndpoint.IsAny && !targetEndpoint.IsLoopback)
                Debug.LogError($"Attempting to [StartClientServer] but with a non-loopback IP!? {targetEndpoint.ToFixedString()}!");
            
            IsTryingToConnect = true;
            if (ClientServerBootstrap.RequestedPlayType != ClientServerBootstrap.PlayType.ClientAndServer)
            {
                Debug.LogError(
                    $"Creating client/server worlds is not allowed if playmode is set to {ClientServerBootstrap.RequestedPlayType}");
                return;
            }

            //Destroy the local simulation world to avoid the game scene to be loaded into it
            //This prevent rendering (rendering from multiple world with presentation is not greatly supported)
            //and other issues.
            DestroyActiveSimulationWorld();

            var server = ClientServerBootstrap.CreateServerWorld("ServerWorld");
            var client = ClientServerBootstrap.CreateClientWorld("ClientWorld");

            World.DefaultGameObjectInjectionWorld ??= server;
            
            SceneController.LoadGame();

            var networkEndpoint = NetworkEndpoint.AnyIpv4.WithPort(targetEndpoint.Port);
            {
                if (targetEndpoint != CommandLineConfig.MultiplayEndpoint && CommandLineConfig.MultiplayEndpoint.IsValid) 
                    Debug.LogError($"Requested to start Server on '{targetEndpoint.ToFixedString()}', but CLI specifies '{CommandLineConfig.MultiplayEndpoint.ToFixedString()}'! Ignoring CLI as user preference takes precidence!");
                using var drvQuery = server.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
                drvQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW.Listen(networkEndpoint);
            }

            networkEndpoint = NetworkEndpoint.LoopbackIpv4.WithPort(targetEndpoint.Port);
            {
                using var drvQuery =
                    client.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
                drvQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW.Connect(client.EntityManager, networkEndpoint);
            }
            IsTryingToConnect = false;
        }
        
        /// <summary>
        ///     Start an in-proc Server.
        /// </summary>
        private static void StartServer(NetworkEndpoint targetEndpoint)
        {
            if(!targetEndpoint.IsAny && !targetEndpoint.IsLoopback)
                Debug.LogError($"Attempting to [StartSeverOnly] but with a non-any IP!? {targetEndpoint.ToFixedString()}!");
            
            IsTryingToConnect = true;

            // Destroy the local simulation world to avoid the game scene to be loaded into it
            // This prevent rendering (rendering from multiple world with presentation is not greatly supported) and other issues.
            DestroyActiveSimulationWorld();

            var server = ClientServerBootstrap.CreateServerWorld("ServerWorld");
 
            World.DefaultGameObjectInjectionWorld ??= server;
            
            SceneController.LoadGame();

            var networkEndpoint = NetworkEndpoint.AnyIpv4.WithPort(targetEndpoint.Port);
            {
                if (targetEndpoint != CommandLineConfig.MultiplayEndpoint) 
                    Debug.LogError($"Requested to start Server on '{targetEndpoint.ToFixedString()}', but CLI specifies '{CommandLineConfig.MultiplayEndpoint.ToFixedString()}'! Ignoring CLI as user preference takes precedence!");
                using var drvQuery = server.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
                drvQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW.Listen(networkEndpoint);
            }

            IsTryingToConnect = false;
        }

        /// <summary>
        ///     Connect to a server via the IP address and port.
        /// </summary>
        private static void ConnectToServer(NetworkEndpoint targetEndpoint)
        {
            IsTryingToConnect = true;
            DestroyActiveSimulationWorld();
            var client = ClientServerBootstrap.CreateClientWorld("ClientWorld");
            World.DefaultGameObjectInjectionWorld ??= client;

            SceneController.LoadGame();

            using var drvQuery =
                client.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            drvQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW.Connect(client.EntityManager, targetEndpoint);
            IsTryingToConnect = false;
        }

        /// <summary>
        /// Destroying a world should happen in a GameObjectLoop instead of ECS systems loop.
        /// </summary>
        private static void DestroyActiveSimulationWorld()
        {
            for (var i = 0; i < World.All.Count; i++)
            {
                var world = World.All[i];
                if ((world.Flags & WorldFlags.Game) != WorldFlags.None && !world.IsThinClient())
                {
                    Debug.Log($"[DestroyActiveSimulationWorld] [{i}] Disposing '{world.Name}'!");
                    world.Dispose();
                    i--;
                }
            }
        }
    }
}