using Unity.Collections;
using Unity.Entities;

namespace Unity.NetCode.Extensions
{
    public struct GFXDeviceInformation : IComponentData
    {
        public int MemorySize;
        public FixedString64Bytes Name;
        public FixedString64Bytes Version;
        public FixedString64Bytes API;
        public FixedString64Bytes Vendor;

        public override string ToString()
        {
            var separator = "\n-----------------------\n";
            var stats = string.Empty;
            stats += $"GPU{separator}";
            stats += $"Name: {Name}\n";
            stats += $"Version: {Version}\n";
            stats += $"GFX API: {API}\n";
            stats += $"Vendor: {Vendor}\n";
            stats += $"Memory Size: {MemorySize} MB\n";
            return stats;
        }
    }

    public struct CPUDeviceInformation : IComponentData
    {
        public FixedString64Bytes Type;
        public int Count;
        public FixedString64Bytes Frequency;

        public override string ToString()
        {
            var separator = "\n-----------------------\n";
            var stats = string.Empty;
            stats += $"{separator}";
            stats += $"CPU{separator}";
            stats += $"Type: {Type}\n";
            stats += $"Cores: {Count}\n";
            stats += $"Frequency: {Frequency}\n";
            return stats;
        }
    }

    public struct MemoryDeviceInformation : IComponentData
    {
        public FixedString64Bytes Size;
        public FixedString64Bytes Allocated;
        public FixedString64Bytes Free;

        public override string ToString()
        {
            var separator = "\n-----------------------\n";
            var stats = string.Empty;
            stats += $"{separator}";
            stats += $"Memory{separator}";
            stats += $"Size: {Size}\n";
            stats += $"Allocated: {Allocated}\n";
            stats += $"Free: {Free}\n";
            return stats;
        }
    }
}
