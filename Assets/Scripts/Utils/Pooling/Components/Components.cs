using System;
using Unity.Entities;

namespace MegacityMetro.Pooling
{
    public struct DynamicInstanceLinkCleanup : ICleanupComponentData
    {
        public int InstanceIndex;
    }
}
