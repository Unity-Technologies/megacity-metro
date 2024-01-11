using Unity.Entities;
using Unity.MegacityMetro.Gameplay;
using UnityEngine;

namespace Unity.MegacityMetro.Authoring
{
    /// <summary>
    /// examines the object's scale and position to generate a cube and
    /// establishes the map's limits according to the cube.
    /// </summary>
    public class LevelBoundsContainerAuthoring : MonoBehaviour
    {
        [SerializeField] public GameObject BoundsPrefab;
        [BakingVersion("megacity-metro", 1)]
        public class LevelBoundsBaker : Baker<LevelBoundsContainerAuthoring>
        {
            public override void Bake(LevelBoundsContainerAuthoring authoring)
            {
                var entity = GetEntity(authoring.gameObject, TransformUsageFlags.Dynamic);
                var boundsEntityPrefab = GetEntity(authoring.BoundsPrefab, TransformUsageFlags.Dynamic);
                AddComponent(entity, new LevelBoundsContainer
                {
                    BoundsPrefab = boundsEntityPrefab
                });
            }
        }
    }
}