using Unity.Entities;
using UnityEngine;

namespace MegacityMetro.Pooling
{
    public class DynamicInstanceLinkAuthoring : MonoBehaviour
    {
        public bool StartDisabled;
        public PoolType PoolType;
        public bool UpdatePosition;
        public bool UpdateRotation;

        [BakingVersion("megacity-metro", 1)]
        private class DynamicInstanceLinkBaker : Baker<DynamicInstanceLinkAuthoring>
        {
            public override void Bake(DynamicInstanceLinkAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new DynamicInstanceLink
                {
                    PoolType = (int)authoring.PoolType,
                    UpdatePosition = authoring.UpdatePosition,
                    UpdateRotation = authoring.UpdateRotation,
                });

                if (authoring.StartDisabled)
                {
                    AddComponent<Disabled>(entity);
                }
            }
        }
    }
    
    public struct DynamicInstanceLink : IComponentData
    {
        public int PoolType;
        public bool UpdatePosition;
        public bool UpdateRotation;
        public bool Linked;
    }
}