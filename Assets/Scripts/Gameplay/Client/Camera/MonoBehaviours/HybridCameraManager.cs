using System;
using Unity.Mathematics;
using Unity.MegacityMetro.Gameplay;
using UnityEngine;
using Utils.Misc;
using UnityEngine.Serialization;

namespace Unity.MegacityMetro.CameraManagement
{
    /// <summary>
    /// Create camera target authoring component in order to
    /// allow the game object camera to follow the player camera target entity
    /// </summary>
    public class HybridCameraManager : MonoBehaviour
    {
        public GameObject CameraObject;

        public float FollowDistance = 25.0f;
        public float FollowSharpness = 5.0f;
        public float LookAtSharpness = 5.0f;
        
        private float3 _playerPosition;
        private float3 _aimPosition;
        private float3 _prevTargetPosition;

        public static HybridCameraManager Instance;
        [NonSerialized]
        public bool WasInitialized;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
        }

        public void Reset()
        {
            WasInitialized = false;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void ResetStaticData()
        {
            Instance = null;
        }
        
        public void UpdateAimCameraTargetPosition(float3 position)
        {
            _aimPosition = position;
        }

        public void SetPlayerTargetPosition(float3 position)
        {
            _playerPosition = position;
        }

        public void StartShaking()
        {
            // TODO
        }

        public void PlaceCamera(float3 position, quaternion rotation)
        {
            _playerPosition = position;
            _aimPosition = _playerPosition + math.mul(rotation, math.forward());
            CameraUpdate(1f, true);
        }

        public void CameraUpdate(float deltaTime, bool skipSmoothing = false)
        {
            Transform cameraTransform = CameraObject.transform;

            float3 lookDirection = math.normalizesafe(_aimPosition - _playerPosition);
            float3 targetPosition = _playerPosition - (lookDirection * FollowDistance);
            float3 camToAimPointDir = math.normalizesafe(_aimPosition - (float3)cameraTransform.position);
            quaternion targetRotation = quaternion.LookRotationSafe(camToAimPointDir, math.up());
            if (skipSmoothing)
            {
                cameraTransform.position = targetPosition;
                cameraTransform.rotation = targetRotation;
            }
            else
            {
                cameraTransform.position = MathUtilities.SmoothFollow(cameraTransform.position, _prevTargetPosition, targetPosition, deltaTime, FollowSharpness);
                cameraTransform.rotation = math.slerp(cameraTransform.rotation, targetRotation,  MathUtilities.GetSharpnessInterpolant(LookAtSharpness, deltaTime));
            }

            _prevTargetPosition = targetPosition;
        }
    }
}
