using Unity.Entities;
using UnityEngine;

namespace Unity.NetCode.Extensions
{
    [DisallowMultipleComponent]
    public class EnableConnectionMonitorAuthoring : MonoBehaviour
    {
        [BakingVersion("megacity-metro", 1)]
        private class EnableConnectionMonitorBaker : Baker<EnableConnectionMonitorAuthoring>
        {
            public override void Bake(EnableConnectionMonitorAuthoring authoring)
            {
                EnableConnectionMonitor component = default(EnableConnectionMonitor);
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, component);
            }
        }
    }
}