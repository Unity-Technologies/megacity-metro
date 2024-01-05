using System.Diagnostics;
using UnityEngine;

using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Unity.NAME.Utilities
{
    // A class with various debug functions

    public class DebugUtils
    {
        [Conditional("UNITY_EDITOR")]
        public static void Trap(bool expression, string message = "")
        {
            if (!expression)
            {
                Debug.LogError("<color=red>" + message + "</color>");
                Debug.Break();
            }
        }

        [Conditional("UNITY_EDITOR")]
        public static void Assert(bool expression, string message = "")
        {
            if (!expression)
            {
                Debug.LogWarning("<color=orange>" + message + "</color>");
            }
        }

        [Conditional("UNITY_EDITOR")]
        public static void Log(string message)
        {
            Debug.Log(message);
        }

        // ===> Use [RequireComponent(typeof(MyComponent)))] instead
        //public static void HandleErrorIfNullGetComponent<TO, TS>(Component component, Component source, GameObject onObject)
        //{
        //    if (component == null)
        //    {
        //        Debug.LogError("Error: Component of type " + typeof(TS) + " on GameObject " + source.gameObject.name +
        //                       " expected to find a component of type " + typeof(TO) + " on GameObject " + onObject.name + ", but none were found.");
        //    }
        //}

        [Conditional("UNITY_EDITOR")]
        public static void HandleErrorIfNullFindObject<TO, TS>(Object obj, Component source)
        {
            if (obj == null)
            {
                Debug.LogError("Error: Component of type " + typeof(TS) + " on GameObject " + source.gameObject.name +
                    " expected to find an object of type " + typeof(TO) + " in the scene, but none were found.");
            }
        }
    }
}
