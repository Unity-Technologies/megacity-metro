using Gameplay;
using JetBrains.Annotations;
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using UnityEngine;
using Unity.MegacityMetro.Gameplay;
using Unity.Services.MultiplayerSDK.Utils;
#if UNITY_SERVER
using Unity.Services.MultiplayerSDK.Server;
#endif

namespace Unity.MegacityMetro
{
    /// <summary>
    /// The bootstrap needs to extend `ClientServerBootstrap`, there can only be one class extending it in the project 
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public class NetCodeBootstrap : ClientServerBootstrap
    {
        const string k_MultiplayerDataResourceLocation = "MultiplayerServerSettings";
        public static NetworkEndpoint MegacityMetroServerIp => NetworkEndpoint.Parse("127.0.0.1", 7979);

        /// <summary>
        /// Limitation imposed to ensure UTP send/receiveQueueSize's are set appropriately.
        /// <see cref="MegacityMetroDriverConstructor"/>.
        /// </summary>
        public const int MaxPlayerCount = 100;

        // The initialize method is what entities calls to create the default worlds
        public override bool Initialize(string defaultWorldName)
        {
            Debug.Log($"Initialize NetcodeBootstrap using world '{defaultWorldName}'");

            // This style of check should be incorporated into the SDK and extended
            // to included additional use cases (e.g. Relay)..
            var context = PlayModeContext.CheckCompatibility();
            if (!context.compatible)
            {
                // Logging an error for visibility, could arguably throw an exception
                Debug.LogError(context.message);
            }

            if (PlayModeContext.IsServer)
            {
                // If Build config has set the Menu scene in there 
                // it will automatically load the Main scene for the server
                if (SceneController.IsFrontEnd)
                {
                    SceneController.LoadGame();
                }

                CreateDefaultClientServerWorlds();
                TryStartUgs();

                return true;
            }
            else 
            {
                // Creates thin clients or worlds for both Editor and Players
                ServerConnectionUtils.CreateDefaultWorld(true);
                if (PlayModeContext.IsEditor)
                {
                    return EditorWorldCreation(defaultWorldName);
                }
                else if (TryCreateThinClientsIfRequested()) 
                {
                    return true;
                }
            }

            return true;
        }

        private bool EditorWorldCreation(string defaultWorldName)
        {
#if UNITY_EDITOR
            if (IsEditorConnectedFromTheMainScene(defaultWorldName))
            {
                return true;
            }
            else if (RequestedPlayType == PlayType.Client &&
                MultiplayerPlayModePreferences.IsEditorInputtedAddressValidForConnect(out var editorSpecifiedEndpoint))
            {
                AutoConnectPort = editorSpecifiedEndpoint.Port;
                DefaultConnectAddress = editorSpecifiedEndpoint;
                Debug.Log($"Detected auto-connection preference in 'Multiplayer PlayMode Tool' targeting '{editorSpecifiedEndpoint}' (Port: '{AutoConnectPort}')!");
            }
#endif
            return true;
        }

        public static void CheckWorlds()
        {
            Debug.Log($"Do worlds exist? Client: {ClientWorld} / Server: {ServerWorld}");
        }

        /// <summary>
        /// Verifies if the current opened scene is the main menu
        /// Otherwise, it will select between single player or multiplayer setup
        /// </summary>
        /// <param name="defaultWorldName"></param>
        /// <returns></returns>
        private bool IsEditorConnectedFromTheMainScene(string defaultWorldName)
        {
            var isMainScene = SceneController.IsGameScene;
#if UNITY_EDITOR
            var gameInfo = Resources.Load<MultiplayerServerSettings>(k_MultiplayerDataResourceLocation);
            if (!gameInfo)
            {
                Debug.LogError($"[EditorSettings] No Game Info Object at 'Assets/Resources/{k_MultiplayerDataResourceLocation}'");
                return false;
            }

            gameInfo.IsAutoloading = isMainScene;

            if (isMainScene)
            {
                var isMultiplayerMode = gameInfo.AutoRunGameModeInEditorMain == GameMode.Multiplayer || gameInfo.AutoRunGameModeInEditorMain == GameMode.None;
                if (isMultiplayerMode)
                {
                    Debug.Log("Auto creating server and client worlds");
                    AutoConnectPort = 7979;
                    CreateDefaultClientServerWorlds();
                    return true;
                }

                // by default it creates a single player mode
                Debug.Log("Auto creating Local world");
                CreateLocalWorld(defaultWorldName);
            }
#endif
            return isMainScene;
        }

        [UsedImplicitly]
        private static bool TryCreateThinClientsIfRequested()
        {
            if (CommandLineConfig.AutomaticallyAddThinClients)
            {
                var requestedNumThinClients = CommandLineConfig.TargetThinClientWorldCount;
#if UNITY_EDITOR
                // CLI argument means that user has requested ThinClients in the editor.
                // Therefore, we MUST clobber MultiplayerPlayModePreferences.RequestedNumThinClients,
                // otherwise MultiplayerPlayModePreferences will immediately destroy the worlds we create.
                // This is technically a bug in Netcode. Reported.
                MultiplayerPlayModePreferences.RequestedNumThinClients = requestedNumThinClients;
#endif
                
                for (var i = 0; i < requestedNumThinClients; i++)
                {
                    Debug.Log($"[TryCreateThinClientsIfRequested] Creating a Thin Client World! {(i + 1)} of {requestedNumThinClients}...");
                    var world = CreateThinClientWorld();
                    if (World.DefaultGameObjectInjectionWorld == null || !World.DefaultGameObjectInjectionWorld.IsCreated)
                    {
                        World.DefaultGameObjectInjectionWorld = world;
                        Debug.Log($"[TryCreateThinClientsIfRequested] Setting DefaultGameObjectInjectionWorld to world '{world.Name}'.");
                    }
                }

                Debug.Log($"[TryCreateThinClientsIfRequested] Automatically created {requestedNumThinClients} ThinClients, and connecting them to either a) the same connection as the main client, otherwise b) a hardcoded endpoint '{DefaultConnectAddress.ToFixedString()}' (Port: {AutoConnectPort})!");
                return true;
            }

            Debug.Log($"[TryCreateThinClientsIfRequested] No ThinClients requested!");
            return false;
        }

        /// <summary>
        /// Runs parallel to the initialization thread from here.
        /// </summary>
        private void TryStartUgs()
        {
#if UNITY_SERVER
            // Defer adding component until needed to avoid race conditions where its logic
            // assumes that the NetcodeBootstrap has already initialized..
            var gameServerManager = new GameObject("MultiplayerServices").AddComponent<ServerBehaviour>();
#endif
        }
    }
}