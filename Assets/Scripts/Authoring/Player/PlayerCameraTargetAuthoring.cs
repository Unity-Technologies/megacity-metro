using Unity.Entities;
using Unity.MegacityMetro.CameraManagement;
using UnityEngine;

namespace Unity.MegacityMetro.Authoring
{
    /// <summary>
    /// Create tag component for the player camera target
    /// </summary>
    public class PlayerCameraTargetAuthoring : MonoBehaviour
    {
        public float VerticalOffset = 1f;

        [BakingVersion("megacity-metro", 2)]
        public class PlayerCameraTargetBaker : Baker<PlayerCameraTargetAuthoring>
        {
            public override void Bake(PlayerCameraTargetAuthoring authoring)
            {
                var entity = GetEntity(authoring.gameObject, TransformUsageFlags.Dynamic);
                AddComponent(entity, new PlayerCameraTarget
                {
                    VerticalOffset = authoring.VerticalOffset,
                });
            }
        }
    }
}
