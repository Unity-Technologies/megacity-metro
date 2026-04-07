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
}
