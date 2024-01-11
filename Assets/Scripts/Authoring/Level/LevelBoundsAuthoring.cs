using Unity.Entities;
using Unity.MegacityMetro.Gameplay;
using UnityEngine;

namespace Unity.MegacityMetro.Authoring
{
    /// <summary>
    /// examines the object's scale and position to generate a cube and
    /// establishes the map's limits according to the cube.
    /// </summary>
    public class LevelBoundsAuthoring : MonoBehaviour
    {
        [SerializeField]
        private float MaxDistance = 1000f;

        [BakingVersion("megacity-metro", 4)]
        public class LevelBoundsBaker : Baker<LevelBoundsAuthoring>
        {
            public override void Bake(LevelBoundsAuthoring authoring)
            {
                var entity = GetEntity(authoring.gameObject, TransformUsageFlags.Dynamic);
                AddComponent(entity, new LevelBounds { MaxDistance = authoring.MaxDistance });
            }
        }
    }
}