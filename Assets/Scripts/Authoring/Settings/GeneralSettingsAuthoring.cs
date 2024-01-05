using Gameplay.Misc.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

namespace Unity.MegacityMetro.Authoring
{
    public class GeneralSettingsAuthoring : MonoBehaviour
    {
        public LayerMask LightsLayer;
        public float MaxLightsDetectionRange = 500f;
        public int MaxLights = 8;
        public float LightsFadeInDuration = 2.5f;
        public float ForwardBias = 0f;
        public float MinDistanceForForwardBias = 20f;

        [BakingVersion("megacity-metro", 1)]
        public class ControlSettingsBaker : Baker<GeneralSettingsAuthoring>
        {
            public override void Bake(GeneralSettingsAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);

                CollisionFilter lightsObjectFilter = CollisionFilter.Default;
                lightsObjectFilter.BelongsTo = 0u;
                lightsObjectFilter.BelongsTo |= (uint)authoring.LightsLayer.value;
                CollisionFilter lightsDetectionFilter = CollisionFilter.Default;
                lightsDetectionFilter.CollidesWith = 0u;
                lightsDetectionFilter.CollidesWith |= (uint)authoring.LightsLayer.value;

                AddComponent(entity, new GeneralSettings
                {
                    LightsDetectionFilter = lightsDetectionFilter,
                    MaxLightsDetectionRange = authoring.MaxLightsDetectionRange,
                    MaxLights = authoring.MaxLights,
                    LightsFadeInDuration = authoring.LightsFadeInDuration,
                    ForwardBias = authoring.ForwardBias,
                    MinDistanceForForwardBias = authoring.MinDistanceForForwardBias,

                    LightsCollider = Unity.Physics.SphereCollider.Create(
                        new SphereGeometry
                        {
                            Center = float3.zero,
                            Radius = 0.1f,
                        },
                        lightsObjectFilter,
                        new Unity.Physics.Material
                        {
                            CollisionResponse = CollisionResponsePolicy.None,
                        })
                });
            }
        }
    }
}