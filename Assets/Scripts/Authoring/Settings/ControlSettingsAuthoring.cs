using Unity.Entities;
using Unity.Mathematics;
using Unity.MegacityMetro.Gameplay;
using UnityEngine;

namespace Unity.MegacityMetro.Authoring
{
    /// <summary>
    /// Authoring component for ControlSettings
    /// </summary>
    public class ControlSettingsAuthoring : MonoBehaviour
    {
        [SerializeField]
        private float MouseSensitivity = 1f;
        [SerializeField]
        private float2 AccelerationRange;

        [BakingVersion("megacity-metro", 2)]
        public class ControlSettingsBaker : Baker<ControlSettingsAuthoring>
        {
            public override void Bake(ControlSettingsAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new ControlSettings
                {
                    MouseSensitivity = authoring.MouseSensitivity,
                    InverseLookHorizontal = false,
                    InverseLookVertical = false,
                    AccelerationRange = authoring.AccelerationRange,
                    AimAssistanceSensitivity = 1,
                });
            }
        }
    }
}