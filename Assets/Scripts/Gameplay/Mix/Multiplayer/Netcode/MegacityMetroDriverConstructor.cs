using Unity.Entities;
using Unity.MegacityMetro;
using Unity.NetCode;
using Unity.Networking.Transport;

// NW added this initially to address performance issues, need to keep
namespace Gameplay
{
    /// <summary>
    /// Register client and server drivers, allowing huge player counts.
    /// </summary>
    public class MegacityMetroDriverConstructor : INetworkStreamDriverConstructor
    {
        public void CreateClientDriver(World world, ref NetworkDriverStore driverStore, NetDebug netDebug)
        {
            var settings = DefaultDriverBuilder.GetNetworkSettings();
            // Left as default: FixSettingsForMegacityMetro(settings, ???);
            DefaultDriverBuilder.RegisterClientUdpDriver(world, ref driverStore, netDebug, settings);
        }

        public void CreateServerDriver(World world, ref NetworkDriverStore driverStore, NetDebug netDebug)
        {
            var settings = DefaultDriverBuilder.GetNetworkServerSettings();
            // https://forum.unity.com/threads/mutiple-errorcode-5-related-warnings.1098229/#post-7731681
            // Assumed 5 packets queued per connection is overkill, but safe.
            // It's only extra memory consumption, which is relatively harmless.
            FixSettingsForMegacityMetro(settings, NetCodeBootstrap.MaxPlayerCount * 5);
            DefaultDriverBuilder.RegisterServerDriver(world, ref driverStore, netDebug, settings);
        }

        private static void FixSettingsForMegacityMetro(NetworkSettings settings, int sendReceiveQueueCapacity)
        {
            if (settings.TryGet(out NetworkConfigParameter networkConfig))
            {
                networkConfig.sendQueueCapacity = networkConfig.receiveQueueCapacity = sendReceiveQueueCapacity;
                settings.AddRawParameterStruct(ref networkConfig);
            }
        }
    }
}
