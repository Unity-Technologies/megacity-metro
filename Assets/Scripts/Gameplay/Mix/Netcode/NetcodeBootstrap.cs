using Gameplay;
using JetBrains.Annotations;
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using UnityEngine;
using Unity.Services.Samples.GameServerHosting;
using Unity.MegacityMetro.Gameplay;

namespace Unity.MegacityMetro
{
    /// <summary>
    /// The bootstrap needs to extend `ClientServerBootstrap`, there can only be one class extending it in the project 
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public class NetCodeBootstrap : ClientServerBootstrap
    {
        const string k_ServicesDataResourceLocation = "GameServiceData";
        const string k_MultiplayerDataResourceLocation = "MultiplayerServerSettings";
        public static NetworkEndpoint MegacityMetroServerIp => NetworkEndpoint.Parse("192.168.200.229", 5010);

        /// <summary>
        /// Limitation imposed to ensure UTP send/receiveQueueSize's are set appropriately.
        /// <see cref="MegacityMetroDriverConstructor"/>.
        /// </summary>
        public const int MaxPlayerCount = 200;

        // The initialize method is what entities calls to create the default worlds
        public override bool Initialize(string defaultWorldName)
        {
            Debug.Log("NetCodeBootstrap Initialize");
            // Handle max player count globally.
            NetworkStreamReceiveSystem.DriverConstructor = new MegacityMetroDriverConstructor();

#if UNITY_SERVER && !UNITY_EDITOR
            var isFrontEnd = SceneController.IsFrontEnd; 
            if(isFrontEnd)
            {
                SceneController.LoadGame();
            }
            
            CreateDefaultClientServerWorlds();
            TryStartUgs();

            // Disabled as UNITY_SERVER does not currently support creating thin client worlds.
            // On the server, also create thin clients (if requested).
            // TryCreateThinClientsIfRequested();

            return true;
#else

            // Try and auto-connect.
#if UNITY_EDITOR
            if (IsConnectedFromTheMainScene(defaultWorldName)) 
            {
                return true;
            }
            
            if (RequestedPlayType == PlayType.Client)
            {
                if (MultiplayerPlayModePreferences.IsEditorInputtedAddressValidForConnect(out var editorSpecifiedEndpoint))
                {
                    AutoConnectPort = editorSpecifiedEndpoint.Port;
                    DefaultConnectAddress = editorSpecifiedEndpoint;
                    Debug.Log($"Detected auto-connection preference in 'Multiplayer PlayMode Tool' targeting '{editorSpecifiedEndpoint}' (Port: '{AutoConnectPort}')!");
                }
            }
#endif
            if (TryCreateThinClientsIfRequested())
                return true;
            
            ServerConnectionUtils.CreateDefaultWorld();
            return true;
#endif
        }

        private bool IsConnectedFromTheMainScene(string defaultWorldName)
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
        private async void TryStartUgs()
        {
            var gameInfo = Resources.Load<GameServerInfo_Data>(k_ServicesDataResourceLocation);
            if (!gameInfo)
            {
                Debug.LogWarning($"[GSH] No Game Server Info Object at 'Assets/Resources/{k_ServicesDataResourceLocation}'");
                return;
            }

            var gameServerManager = new GameObject("MultiplayServer").AddComponent<GameServerManager>();
            gameServerManager.Init(gameInfo);
            Debug.Log($"[GSH] Multiplay GameServer Manager {gameInfo.Data}'");
            
            if (!await gameServerManager.InitServices())
                return;
            
            Debug.Log($"[GSH] Try Start Server {CommandLineConfig.MultiplayEndpoint.ToFixedString()}!");
            await gameServerManager.TryStartServer(CommandLineConfig.MultiplayEndpoint.Address);
        }
    }
}