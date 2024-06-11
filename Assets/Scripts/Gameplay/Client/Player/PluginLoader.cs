using System;
using System.IO;
using UnityEngine;
#if !UNITY_EDITOR_WIN && UNITY_STANDALONE_WIN
using System.Runtime.InteropServices;
#endif

namespace Unity.MegacityMetro.Gameplay
{
    public static class PluginLoader
    {
        private static bool IsPluginLoaded = false;
#if UNITY_EDITOR_OSX
        [DllImport("TrackpadAccessEditor.dylib")]
        private static extern int TrackpadTouchesCount();
#elif UNITY_STANDALONE_OSX
        [DllImport("TrackpadAccess.bundle")]
        private static extern int TrackpadTouchesCount();
#elif !UNITY_EDITOR_WIN && UNITY_STANDALONE_WIN
        [DllImport("TrackpadAccess.dll")]
        private static extern int TrackpadTouchesCount();
    
        [DllImport("TrackpadAccess.dll")]
        private static extern int InitTrackpadCommunication();
    
        [DllImport("TrackpadAccess.dll")]
        private static extern int CloseTrackpadCommunication();
#else
        private static int TrackpadTouchesCount()
        {
            return 0;
        }
#endif

        public static int TouchesCount()
        {
            if (IsPluginLoaded)
                return TrackpadTouchesCount();
            
            return 0;
        }

        public static void UnloadPlugin()
        {
#if !UNITY_EDITOR_WIN && UNITY_STANDALONE_WIN
            if (IsPluginLoaded)
                CloseTrackpadCommunication();
#endif
                
        }

        private static bool IsPluginAvailable(string pluginName)
        {
            var root = "Plugins";
            if (Application.platform == RuntimePlatform.OSXPlayer)
                root = "../Frameworks/Plugins";
            if (Application.platform == RuntimePlatform.OSXEditor)
                root = "Plugins/macOS";
            else if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
            {
                var architecture = IntPtr.Size == 8 ? "x64" : "x86";
                root = Path.Combine("Plugins/Windows", architecture);
            }
            
            
            var pluginPath = Path.Combine(Application.dataPath, root, pluginName);
            return File.Exists(pluginPath);
        }

        public static void LoadPlugin()
        {
            var pluginName = "";

            if (Application.platform == RuntimePlatform.WindowsEditor)
                pluginName = "TrackpadAccess.dll";
            else if (Application.platform == RuntimePlatform.OSXPlayer)
                pluginName = "TrackpadAccess.bundle";
            else if (Application.platform == RuntimePlatform.OSXEditor)
                pluginName = "TrackpadAccessEditor.dylib";
            else
                return;
            
            if (IsPluginAvailable(pluginName))
            {
                IsPluginLoaded = true;
                try
                {
#if !UNITY_EDITOR_WIN && UNITY_STANDALONE_WIN
                    InitTrackpadCommunication();
#endif
                }
                catch (DllNotFoundException e)
                {
                    IsPluginLoaded = false;
                    Debug.Log($"Couldn't load Plugin: {e.Message}");
                }
                catch (Exception e)
                {
                    IsPluginLoaded = false;
                    Debug.LogError($"Execution Error Plugin: {e.Message}");
                }
            }
            else
            {
                IsPluginLoaded = false;
                Debug.LogWarning($"Cannot be found '{pluginName}'.");
            }
        }
    }
}