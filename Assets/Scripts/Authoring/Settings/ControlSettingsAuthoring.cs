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
        private float WinTrackpadSensitivity = 1f;
        [SerializeField]
        private float MacTrackpadSensitivity = 1f;
        [SerializeField]
        private float MouseSensitivity = 1f;
        [SerializeField]
        private float2 AccelerationRange;

        [BakingVersion("megacity-metro", 3)]
        public class ControlSettingsBaker : Baker<ControlSettingsAuthoring>
        {
            public override void Bake(ControlSettingsAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new ControlSettings
                {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                    TrackpadSensitivity = authoring.WinTrackpadSensitivity,
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                    TrackpadSensitivity = authoring.MacTrackpadSensitivity,
#else
                    TrackpadSensitivity = 1,
#endif
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