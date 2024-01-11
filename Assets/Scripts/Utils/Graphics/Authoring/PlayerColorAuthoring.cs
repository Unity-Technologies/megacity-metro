using Unity.Entities;
using UnityEngine;

namespace Unity.MegacityMetro.Utils
{
    /// <summary>
    /// Adds the SetPlayerColorTag which identifies an entity that is missing a color definition.
    /// </summary>
    public class PlayerColorAuthoring : MonoBehaviour
    {
        [BakingVersion("megacity-metro", 3)]
        private class PlayerColorBaker : Baker<PlayerColorAuthoring>
        {
            public override void Bake(PlayerColorAuthoring authoring)
            {
                var entity = GetEntity(authoring.gameObject, TransformUsageFlags.Dynamic);
                AddComponent<SetPlayerColorTag>(entity);
            }
        }
    }
}
