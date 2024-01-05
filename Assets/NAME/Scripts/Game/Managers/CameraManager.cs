using System;
using System.Collections.Generic;
using Unity.NAME.Utilities;
using UnityEngine;

namespace Unity.NAME.Game
{
    // This is an example of a manager for the Cameras that uses the ServiceLocator

    [DefaultExecutionOrder(-100)]
    public class CameraManager : MonoBehaviour, ICameraService
    {
        Dictionary<Type, BaseCamera> m_Cameras;

        void Awake()
        {
            ServiceLocator.RegisterService<ICameraService>(this);
            m_Cameras = new Dictionary<Type, BaseCamera>();
        }

        public void RegisterCamera<T>(T camera) where T : BaseCamera
        {
            if (!m_Cameras.ContainsKey(typeof(T)))
            {
                m_Cameras.Add(typeof(T), camera);
            }
            else
            {
                DebugUtils.Assert(false, $"{typeof(T).Name} Camera already registered");
            }
        }

        public T GetCamera<T>() where T : BaseCamera
        {
            m_Cameras.TryGetValue(typeof(T), out BaseCamera baseCamera);
            DebugUtils.Assert(baseCamera != null, $"{typeof(T).Name} Camera not found");
            return baseCamera as T;
        }

        public void UnregisterCamera<T>(T camera) where T : BaseCamera
        {
            if (m_Cameras.ContainsKey(typeof(T)))
            {
                m_Cameras.Remove(typeof(T));
            }
            else
            {
                DebugUtils.Assert(false, $"{typeof(T).Name} Camera not found");
            }
        }

        public void InitializeService()
        {
            // Do things that needs to be done for initialization
        }

        public void ShutdownService()
        {
            // Do cleansing
        }

        void OnDestroy()
        {
            ServiceLocator.UnregisterService<ICameraService>(this);
        }
    }
}
