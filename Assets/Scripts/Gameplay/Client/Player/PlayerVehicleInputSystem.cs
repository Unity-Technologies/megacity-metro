using Unity.Entities;
using Unity.Mathematics;
using Unity.MegacityMetro.CameraManagement;
using UnityEngine.UIElements;
#if UNITY_ANDROID || UNITY_IPHONE || ENABLED_VIRTUAL_JOYSTICK
using UnityEngine;
using Unity.MegacityMetro.UI;
#else
using UnityEngine;
#endif
using Unity.NetCode;

namespace Unity.MegacityMetro.Gameplay
{
    /// <summary>
    /// System to collect the player input and send it to the player vehicle.
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    [UpdateInGroup(typeof(GhostInputSystemGroup))]
    public partial class PlayerVehicleInputSystemBase : SystemBase
    {
        private GameInput m_GameInput;

        protected override void OnCreate()
        {
            base.OnCreate();
            RequireForUpdate<ControlSettings>();
        }

        protected override void OnStopRunning()
        {
            base.OnStopRunning();
            m_GameInput?.Disable();
        }

        protected override void OnUpdate()
        {
            const float ConstLookVelocityMultiplier = 100f;
            
            if (HybridCameraManager.Instance == null)
                return;

            float deltaTime = SystemAPI.Time.DeltaTime;
            
            if (m_GameInput == null)
            {
                m_GameInput = new GameInput();
                m_GameInput.Enable();
                m_GameInput.Gameplay.Enable();
            }

            var controlSettings = SystemAPI.GetSingleton<ControlSettings>();
            var invertHorizontal = controlSettings.InverseLookHorizontal ? -1 : 1;
            var invertVertical = controlSettings.InverseLookVertical ? -1 : 1;
            var accelerationRange = controlSettings.AccelerationRange;

#if UNITY_ANDROID || UNITY_IPHONE || ENABLED_VIRTUAL_JOYSTICK
            var input = new PlayerVehicleInput
            {
                Acceleration = math.clamp(MobileControls.Instance.Speed, accelerationRange.x, accelerationRange.y),
                LookVelocity = new float2(
                    MobileControls.Instance.JoystickLeft.Delta.y * invertVertical,
                    MobileControls.Instance.JoystickLeft.Delta.x * invertHorizontal) * ConstLookVelocityMultiplier,
                Shoot = MobileControls.Instance.Shoot,
                AimAssistSensitivity = controlSettings.AimAssistanceSensitivity,
            };
#else
            
            var gameplayInputActions = m_GameInput.Gameplay;
            
            float2 lookInput = default; 
            if (math.any(gameplayInputActions.LookDelta.ReadValue<Vector2>()))
            {
                if (deltaTime != 0f)
                {
                    lookInput = new float2
                    {
                        x = -gameplayInputActions.LookDelta.ReadValue<Vector2>().y * invertVertical,
                        y = gameplayInputActions.LookDelta.ReadValue<Vector2>().x * invertHorizontal,
                    };
                    lookInput *= controlSettings.MouseSensitivity;
                    lookInput /= deltaTime; // convert delta displacement this frame to a velocity
                }
            }
            else
            {
                lookInput = new float2
                {
                    x = -gameplayInputActions.LookConst.ReadValue<Vector2>().y * invertVertical,
                    y = gameplayInputActions.LookConst.ReadValue<Vector2>().x * invertHorizontal,
                };
                lookInput *= ConstLookVelocityMultiplier;
            }
            
            var input = new PlayerVehicleInput
            {
                Acceleration = math.clamp(gameplayInputActions.Move.ReadValue<float>(), accelerationRange.x, accelerationRange.y), 
                Roll = gameplayInputActions.Roll.ReadValue<float>(),
                LookVelocity = lookInput,
                Shoot = gameplayInputActions.Fire.IsPressed(),
                AimAssistSensitivity = controlSettings.AimAssistanceSensitivity,
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                Cheat_1 = gameplayInputActions.Cheat_1.IsPressed()
#endif
            };
#endif

            var job = new PlayerVehicleInputJob { CollectedInput = input};
            job.Schedule();
        }
    }

    [WorldSystemFilter(WorldSystemFilterFlags.LocalSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
    public partial class PlayerVehicleInputSystemSinglePlayer : PlayerVehicleInputSystemBase
    { 
    }
}