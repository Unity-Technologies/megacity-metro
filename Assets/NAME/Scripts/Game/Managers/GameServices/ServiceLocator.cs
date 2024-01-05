using System;
using System.Collections.Generic;
using Unity.NAME.Utilities;

namespace Unity.NAME.Game
{
    // A simple ServiceLocator: It could be used to provide a way to gather services without
    // creating hard dependencies between systems.
    // This could be used instead of FindObjectOfType
    // There is an example on how to use it with the CameraManager

    public static class ServiceLocator
    {
        static readonly Dictionary<Type, IGameService> s_Services = new Dictionary<Type, IGameService>();

        public static void RegisterService<T>(T service) where T : IGameService
        {
            DebugUtils.Trap(service != null, "Trying to register a null service");
            if (!s_Services.ContainsKey(typeof(T)))
            {
                s_Services.Add(typeof(T), service);
            }
            else
            {
                DebugUtils.Assert(false, $"{s_Services[typeof(T)].ToString()} already registered, Service will be overwritten with {service.ToString()}!");
                s_Services[typeof(T)] = service;
            }

            service.InitializeService();
        }

        public static void UnregisterService<T>(T service) where T : IGameService
        {
            if (s_Services.ContainsKey(typeof(T)))
            {
                if (s_Services[typeof(T)] == (IGameService)service)
                {
                    service.ShutdownService();
                    s_Services.Remove(typeof(T));
                }
                else
                {
                    DebugUtils.Trap(false, $"Unregistration problem: {s_Services[typeof(T)].ToString()} is not {service.ToString()}!");
                }
            }
        }

        public static T GetService<T>() where T : IGameService
        {
            s_Services.TryGetValue(typeof(T), out IGameService service);
            DebugUtils.Trap(service != null, $"Trying to Get an Invalid Service ({typeof(T)})");
            return (T)service;
        }

        public static void Clear()
        {
            s_Services.Clear();
        }
    }
}
