using UnityEditor; // Don't think we can rely on this shipped in runtime code?
using UnityEngine;
using Unity.NetCode;

namespace Unity.Services.MultiplayerSDK.Utils
{
    public class PlayModeContext
    {
        public static bool IsEditor => Application.platform == RuntimePlatform.LinuxEditor
                                        || Application.platform == RuntimePlatform.WindowsEditor
                                        || Application.platform == RuntimePlatform.OSXEditor;

        public static bool IsServer => Application.platform == RuntimePlatform.LinuxServer
                                        || Application.platform == RuntimePlatform.WindowsServer
                                        || Application.platform == RuntimePlatform.OSXServer;

        // This matters in the editor context, which may be tested as client, server, or both
        // but will not exclusively register as any of those via Application.platform
        public static bool IsServerContext
        {
            get {
                #if UNITY_SERVER
                    return true;
                #endif
                return IsServer;
            }
        }

        // TODO: Include IsClient heuristic
        public static string DebugInfo = $"Platform: {Application.platform} / Server (implicit): {IsServerContext} / Editor: {IsEditor} / Server: {IsServer}";

        // Flag mismatches between build target and PlayMode Type which may lead to missing worlds and
        // similar problematic behaviour! Returns boolean and actionable message to resolve the issue.
        public static (bool compatible, string message) CheckCompatibility()
        {
            bool compatible = true;
            string message = "";
            if (IsEditor)
            {
                var playType = ClientServerBootstrap.RequestedPlayType;

                if (IsServerContext && playType == ClientServerBootstrap.PlayType.Client)
                {
                    message = $"Active build target is a server while PlayMode Type in PlayMode Tools is set to {ClientServerBootstrap.PlayType.Client} - expected client world will NOT be created";
                    compatible = false;
                }

                if (!IsServerContext && playType == ClientServerBootstrap.PlayType.Server)
                {
                    message = $"Active build target is a client while PlayMode Type in PlayMode Tools is set to {ClientServerBootstrap.PlayType.Server} - expected server world will NOT be created";
                    compatible = false;
                }
            }

            return ( compatible, message );
        }
    }
}