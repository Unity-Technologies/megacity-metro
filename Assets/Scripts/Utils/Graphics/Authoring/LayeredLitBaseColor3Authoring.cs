using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Unity.MegacityMetro.Utils
{
    /// <summary>
    /// Adds LayeredLitBaseColor3 to define the player's color.
    /// </summary>
    [DisallowMultipleComponent]
    public class LayeredLitBaseColor3Authoring : MonoBehaviour
    {
        [RegisterBinding(typeof(LayeredLitBaseColor3), nameof(LayeredLitBaseColor3.Value))]
        public float4 Value;

        [BakingVersion("megacity-metro", 2)]
        private class LayeredLitBaseColor3Baker : Baker<LayeredLitBaseColor3Authoring>
        {
            public override void Bake(LayeredLitBaseColor3Authoring authoring)
            {
                var entity = GetEntity(authoring.gameObject, TransformUsageFlags.Dynamic);
                var component = default(LayeredLitBaseColor3);
                component.Value = authoring.Value;
                AddComponent(entity, component);
            }
        }
    }
}
