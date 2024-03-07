using Unity.Entities;
using UnityEngine;

public class AnimatedTimeAuthoring : MonoBehaviour
{
    public Material[] Materials;

    [BakingVersion("megacity-metro", 1)]
    class Baker : Baker<AnimatedTimeAuthoring>
    {
        public override void Bake(AnimatedTimeAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.None);
            AddComponent(entity, new AnimatedTimeSingleton());
            AddComponentObject(entity, new AnimatedTime
            {
                Materials = authoring.Materials,
            });
        }
    }
}
