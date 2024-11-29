// Copy of internal reference
using Unity.Entities;
using Unity.MegacityMetro;
using Unity.NetCode;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Relay;
using Unity.Services.Multiplayer;

namespace Unity.Services.MultiplayerSDK.Client
{
    public class CustomDriverConstructor : INetworkStreamDriverConstructor
    {
        public NetworkConfiguration Configuration;
        public const int InvalidDriverId = 0;

        public int ClientIpcDriverId { get; private set; } = InvalidDriverId;
        public int ClientUdpDriverId { get; private set; } = InvalidDriverId;
        public int ClientWebSocketDriverId { get; private set; } = InvalidDriverId;

        public int ServerIpcDriverId { get; private set; } = InvalidDriverId;
        public int ServerUdpDriverId { get; private set; } = InvalidDriverId;
        public int ServerWebSocketDriverId { get; private set; } = InvalidDriverId;

        public void CreateClientDriver(World world, ref NetworkDriverStore driverStore, NetDebug netDebug)
        {
#if !UNITY_WEBGL
            var ipcSettings = DefaultDriverBuilder.GetNetworkSettings();

            var driverId = 1;

            if (Configuration.Role == NetworkRole.Host || Configuration.Role == NetworkRole.Server)
            {
                UnityEngine.Debug.Log($"RegisterClientIpcDriver");
                DefaultDriverBuilder.RegisterClientIpcDriver(world, ref driverStore, netDebug, ipcSettings);
                ClientIpcDriverId = driverId;
            }
            else if (Configuration.Role == NetworkRole.Client)
            {
                var udpSettings = DefaultDriverBuilder.GetNetworkSettings();

                if (Configuration.Type == NetworkType.Relay)
                {
                    var relayClientData = Configuration.RelayClientData;
                    udpSettings.WithRelayParameters(ref relayClientData);
                }

                UnityEngine.Debug.Log($"RegisterClientUdpDriver");
                DefaultDriverBuilder.RegisterClientUdpDriver(world, ref driverStore, netDebug, udpSettings);
                ClientUdpDriverId = driverId;
            }
#endif
        }

        public void CreateServerDriver(World world, ref NetworkDriverStore driverStore, NetDebug netDebug)
        {
#if !UNITY_WEBGL
            var ipcSettings = DefaultDriverBuilder.GetNetworkSettings();

            var driverId = 1;

            if (Configuration.Role == NetworkRole.Host)
            {
                UnityEngine.Debug.Log($"RegisterServerIpcDriver");
                DefaultDriverBuilder.RegisterServerIpcDriver(world, ref driverStore, netDebug, ipcSettings);
                ServerIpcDriverId = driverId;
                driverId++;
            }

            var udpSettings = DefaultDriverBuilder.GetNetworkSettings();

            if (Configuration.Type == NetworkType.Relay)
            {
                var relayServerData = Configuration.RelayServerData;
                udpSettings.WithRelayParameters(ref relayServerData);
            }

            UnityEngine.Debug.Log($"RegisterServerUdpDriver");
            // TODO: enabled this
            // https://forum.unity.com/threads/mutiple-errorcode-5-related-warnings.1098229/#post-7731681
            // Assumed 5 packets queued per connection is overkill, but safe.
            // It's only extra memory consumption, which is relatively harmless.
            //
            // TODO: Uncomment to restore NW's previous changes
            UpdateSettings(udpSettings, NetCodeBootstrap.MaxPlayerCount * 5);
            DefaultDriverBuilder.RegisterServerUdpDriver(world, ref driverStore, netDebug, udpSettings);
            ServerUdpDriverId = driverId;
#endif
        }

        private static void UpdateSettings(NetworkSettings settings, int sendReceiveQueueCapacity)
        {
            if (settings.TryGet(out NetworkConfigParameter networkConfig))
            {
                networkConfig.sendQueueCapacity = networkConfig.receiveQueueCapacity = sendReceiveQueueCapacity;
                settings.AddRawParameterStruct(ref networkConfig);
            }
        }
    }
}