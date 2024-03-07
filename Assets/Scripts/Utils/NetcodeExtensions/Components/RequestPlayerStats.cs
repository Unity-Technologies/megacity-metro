using Unity.Collections;
using Unity.Entities;

namespace Unity.NetCode.Extensions
{
    public struct RequestPlayerStats : IRpcCommand
    {
        public PlayerStats PlayerStats;
        public GFXDeviceInformation GfxDeviceinfo;
        public CPUDeviceInformation CPUDeviceInfo;
        public MemoryDeviceInformation MemoryDeviceInfo;

        public override string ToString()
        {
            return PlayerStats.ToString() + GfxDeviceinfo.ToString() + CPUDeviceInfo.ToString() + MemoryDeviceInfo.ToString();
        }
    }

    public readonly partial struct RequestPlayerStatsAspect : IAspect
    {
        public readonly Entity Self;
        readonly RefRO<RequestPlayerStats> m_SpawnRequest;
        readonly RefRO<Unity.NetCode.ReceiveRpcCommandRequest> m_ReceiveRpcRequest;
        public string Data => m_SpawnRequest.ValueRO.ToString();
        public string PlayerName => m_SpawnRequest.ValueRO.PlayerStats.Name.ToString();
        public Entity SourceConnection => m_ReceiveRpcRequest.ValueRO.SourceConnection;
    }
}
