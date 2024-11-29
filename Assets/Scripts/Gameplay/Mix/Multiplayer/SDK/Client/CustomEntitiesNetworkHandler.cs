// Copy of internal reference
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using Unity.Services.Multiplayer;

namespace Unity.Services.MultiplayerSDK.Client
{
    class CustomEntitiesNetworkHandler : INetworkHandler
    {
        NetworkConfiguration m_Configuration;
        World m_ClientWorld;
        World m_ServerWorld;

        NetworkStreamDriver m_ClientDriver;
        NetworkStreamDriver m_ServerDriver;
        Entity m_ConnectionEntity;

        public NetworkEndpoint ConnectionEndpoint { private set; get; }

        readonly CustomDriverConstructor m_DriverConstructor = new CustomDriverConstructor();

        public CustomEntitiesNetworkHandler()
        {
        }

        public async Task StartAsync(NetworkConfiguration configuration)
        {
            m_Configuration = configuration;

            SetupWorlds();
            SetupDriverConstructor();

            switch (m_Configuration.Role)
            {
                case NetworkRole.Client:
                    await ConnectAsync();
                    break;
                case NetworkRole.Host:
                    Listen();
                    await SelfConnectAsync();
                    break;
                case NetworkRole.Server:
                    Listen();
                    break;
            }
        }

        public Task StopAsync()
        {
            CleanupClient();
            CleanupServer();
            return Task.CompletedTask;
        }

        void SetupWorlds()
        {
            m_ClientWorld = ClientServerBootstrap.ClientWorld;
            m_ServerWorld = ClientServerBootstrap.ServerWorld;
        }

        void SetupDriverConstructor()
        {
            m_DriverConstructor.Configuration = m_Configuration;
            NetworkStreamReceiveSystem.DriverConstructor = m_DriverConstructor;
        }

        void SetupClientDriver()
        {
            using var drvQuery = m_ClientWorld.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            m_ClientDriver = drvQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW;

            using (var debugQuery = m_ClientWorld.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetDebug>()))
            {
                var netDebug = debugQuery.GetSingleton<NetDebug>();
                var driverStore = new NetworkDriverStore();
                NetworkStreamReceiveSystem.DriverConstructor.CreateClientDriver(m_ClientWorld, ref driverStore, netDebug);
                m_ClientDriver.ResetDriverStore(m_ClientWorld.Unmanaged, ref driverStore);
            }
        }

        void SetupServerDriver()
        {
            using var drvQuery = m_ServerWorld.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            m_ServerDriver = drvQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW;

            using (var debugQuery = m_ServerWorld.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetDebug>()))
            {
                var netDebug = debugQuery.GetSingleton<NetDebug>();
                var driverStore = new NetworkDriverStore();
                NetworkStreamReceiveSystem.DriverConstructor.CreateServerDriver(m_ServerWorld, ref driverStore, netDebug);
                m_ServerDriver.ResetDriverStore(m_ServerWorld.Unmanaged, ref driverStore);
            }
        }

        void Listen()
        {
            ValidateWorld(m_ServerWorld);
            SetupServerDriver();

            NetworkEndpoint listenEndpoint = default;

            switch (m_Configuration.Type)
            {
                case NetworkType.Direct:
                    listenEndpoint = m_Configuration.DirectNetworkListenAddress;
                    break;
                case NetworkType.Relay:
                    listenEndpoint = NetworkEndpoint.AnyIpv4;
                    break;
            }

            if (!listenEndpoint.IsValid)
            {
                throw new Exception("Invalid endpoint to listen to");
            }

            UnityEngine.Debug.Log($"ServerDriver Listen: {listenEndpoint}");

            if (m_ServerDriver.Listen(listenEndpoint))
            {
                var serverUdpPort = m_ServerDriver.GetLocalEndPoint(m_DriverConstructor.ServerUdpDriverId).Port;
                UnityEngine.Debug.Log($"ServerDriver[Udp]: {serverUdpPort}");

                if (m_Configuration.Type == NetworkType.Direct)
                {
                    m_Configuration.UpdatePublishPort(serverUdpPort);
                }
            }
            else
            {
                throw new Exception("Listen Failed");
            }
        }

        async Task SelfConnectAsync()
        {
            ValidateWorld(m_ClientWorld);
            SetupClientDriver();

            var ipcPort = m_ServerDriver.GetLocalEndPoint(m_DriverConstructor.ServerIpcDriverId).Port;
            UnityEngine.Debug.Log($"ServerDriver[Ipc]: {m_ServerDriver.GetLocalEndPoint(m_DriverConstructor.ServerIpcDriverId).Port}");

            var selfEndpoint = NetworkEndpoint.LoopbackIpv4.WithPort(ipcPort);
            UnityEngine.Debug.Log($"ClientDriver SelfConnect: {selfEndpoint}");
            m_ConnectionEntity = m_ClientDriver.Connect(m_ClientWorld.EntityManager, selfEndpoint);
            await ValidateConnectionAsync();
        }

        async Task ConnectAsync()
        {
            ValidateWorld(m_ClientWorld);
            SetupClientDriver();

            NetworkEndpoint connectEndpoint = default;

            switch (m_Configuration.Type)
            {
                case NetworkType.Direct:
                    {
                        connectEndpoint = m_Configuration.DirectNetworkPublishAddress;
                        break;
                    }
                case NetworkType.Relay:
                    {
                        connectEndpoint = m_Configuration.RelayClientData.Endpoint;
                        break;
                    }
            }

            if (!connectEndpoint.IsValid)
            {
                throw new Exception("Invalid endpoint to connect to");
            }

            ConnectionEndpoint = connectEndpoint;
            UnityEngine.Debug.Log($"ClientDriver Connect: {connectEndpoint}");
            m_ConnectionEntity = m_ClientDriver.Connect(m_ClientWorld.EntityManager, connectEndpoint);
            await ValidateConnectionAsync();
        }

        async Task ValidateConnectionAsync()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var connection = m_ClientWorld.EntityManager.GetComponentData<NetworkStreamConnection>(m_ConnectionEntity);

            while (connection.CurrentState != ConnectionState.State.Connected &&
                   connection.CurrentState != ConnectionState.State.Disconnected)
            {
                if (stopwatch.Elapsed.TotalSeconds >= 10)
                {
                    CleanupClient();
                    throw new Exception("Connection timeout. Failed connection");
                }

                await Task.Delay(100);

                if (!m_ClientWorld.EntityManager.Exists(m_ConnectionEntity))
                {
                    throw new Exception("Connect Entity no longer exists. Failed connection");
                }

                connection = m_ClientWorld.EntityManager.GetComponentData<NetworkStreamConnection>(m_ConnectionEntity);
            }
        }

        void CleanupClient()
        {
            if (m_ClientWorld != null &&
                m_ClientWorld.IsCreated &&
                m_ClientWorld.EntityManager.Exists(m_ConnectionEntity))
            {
                m_ClientWorld.EntityManager.AddComponent<NetworkStreamRequestDisconnect>(m_ConnectionEntity);
                m_ConnectionEntity = default;
            }
        }

        void CleanupServer()
        {
        }

        void ValidateWorld(World world)
        {
            if (world == null || !world.IsCreated)
            {
                throw new Exception("Invalid world to setup network");
            }
        }
    }
}