using Unity.Entities;
using UnityEngine;

namespace Unity.NetCode.Extensions
{
    /// <summary>
    /// Adds PlayerName component to the entity
    /// </summary>
    public class PlayerNameAuthoring : MonoBehaviour
    {
        [BakingVersion("megacity-metro", 2)]
        private class PlayerNameBaker : Baker<PlayerNameAuthoring>
        {
            public override void Bake(PlayerNameAuthoring authoring)
            {
                var entity = GetEntity(authoring.gameObject, TransformUsageFlags.Dynamic);
                AddComponent(entity, new PlayerName());
            }
        }
    }
}
