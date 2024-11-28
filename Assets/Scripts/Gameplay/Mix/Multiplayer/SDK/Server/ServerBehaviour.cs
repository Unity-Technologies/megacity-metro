#if UNITY_SERVER
using Unity.Services.MultiplayerSDK.Utils;
using Unity.Services.Samples.GameServerHosting;
using UnityEngine;
using Unity.MegacityMetro;
using System.Threading.Tasks;
using Gameplay.Mix.Multiplayer.SDK.Server;

namespace Unity.Services.MultiplayerSDK.Server
{
    public class ServerBehaviour : MonoBehaviour
    {
        const string k_ServicesDataResourceLocation = "GameServiceData";
        private string apiKeyId = "";
        private string apiKeySecret = "";
        private Dispatcher Dispatcher;
        private ServerManager ServerManager;

        private async Task Awake()
        {
            DontDestroyOnLoad(this);
            ReadAndSetKeysFromCommandLine();

            var gameServerInfo = Resources.Load<GameServerInfo_Data>(k_ServicesDataResourceLocation);
            if (!gameServerInfo)
            {
                Debug.LogWarning($"[ServerBehaviour] No Game Server Info object at 'Assets/Resources/{k_ServicesDataResourceLocation}'");
            }

            Dispatcher = new Dispatcher();
            ServerManager = new ServerManager(Dispatcher, gameServerInfo);
            ServerManager.SetCredentials(apiKeyId, apiKeySecret);
            await ServerManager.StartAsync();
        }

        private void ReadAndSetKeysFromCommandLine()
        {
            if (string.IsNullOrEmpty(CommandLineConfig.MultiplayDeveloperApiKeyIdentifier) || string.IsNullOrEmpty(CommandLineConfig.MultiplayDeveloperApiKeySecret))
            {
                Debug.Log("No service account credentials provided via CLI..");
            }
            else
            {
                Debug.Log("Found service account credentials provided via CLI..");
                apiKeyId = CommandLineConfig.MultiplayDeveloperApiKeyIdentifier;
                apiKeySecret = CommandLineConfig.MultiplayDeveloperApiKeySecret;
            }

        }

        private void Update()
        {
            Dispatcher.Update();
        }
    }
}
#endif