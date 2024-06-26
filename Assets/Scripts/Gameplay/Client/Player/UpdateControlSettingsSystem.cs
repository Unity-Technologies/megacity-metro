﻿using Unity.Entities;
using Unity.MegacityMetro.UI;
using static Unity.Entities.SystemAPI;

namespace Unity.MegacityMetro.Gameplay
{
    /// <summary>
    /// Updates Control Settings based on the Settings UI
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.LocalSimulation)]
    public partial struct UpdateControlSettingsSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ControlSettings>();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (UIControlSettings.Instance == null || !UIControlSettings.Instance.ShouldUpdate)
                return;
            
            var controlSettings = GetSingletonRW<ControlSettings>().ValueRW;
            controlSettings.MouseSensitivity = UIControlSettings.Instance.MouseSensitivity;
            controlSettings.InverseLookHorizontal = UIControlSettings.Instance.InverseLookHorizontal;
            controlSettings.InverseLookVertical = UIControlSettings.Instance.InverseLookVertical;
            controlSettings.AimAssistanceSensitivity = UIControlSettings.Instance.AimAssistanceSensitivity;
            UIControlSettings.Instance.ShouldUpdate = false;
            SetSingleton(controlSettings);
        }
    }
}