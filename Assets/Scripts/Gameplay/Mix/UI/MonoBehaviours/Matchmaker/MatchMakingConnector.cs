using System;
using System.Threading.Tasks;
using Unity.MegacityMetro.Gameplay;
using Unity.MegacityMetro.UI;
using Unity.NetCode.Extensions;
using Unity.Services.Samples;
using Unity.Services.Samples.GameServerHosting;
using UnityEngine;

namespace Unity.MegacityMetro.UGS
{
    /// <summary>
    /// Matchmaking Manager
    /// </summary>
    public class MatchMakingConnector : MonoBehaviour
    {
        public bool ClientIsInGame { get; set; }
        public bool IsTryingToConnect { get; private set; }
        public bool IsMatchmakerInitialized => m_Matchmaker != null;
        [SerializeField] private PlayerInfoItemSettings m_Settings;
        [field: SerializeField] public string IP { get; private set; } = NetCodeBootstrap.MegacityMetroServerIp.Address;
        [field: SerializeField] public ushort Port { get; private set; } = NetCodeBootstrap.MegacityMetroServerIp.Port;

        private ClientMatchmaker m_Matchmaker;
        private PlayerAuthentication m_ProfileService;
        public static MatchMakingConnector Instance { get; private set; }
        public bool HasMatchmakingSuccess { get; private set; }
        public bool IsInitialized { get; private set; }

        private async void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // Check if the project is linked to a project ID
            if (string.IsNullOrEmpty(Application.cloudProjectId))
            {
                Debug.LogWarning($"To use Unity's dashboard services, " +
                                 "you need to link your Unity project to a project ID. " +
                                 "To do this, go to Project Settings to select your organization, " +
                                 "select your project and then link a project ID. " +
                                 "You also need to make sure your organization has access to the required products. " +
                                 "Visit https://dashboard.unity3d.com to sign up.");
                IsInitialized = false;
            }
            else
            {
                await Init();
            }
        }

        private async Task Init()
        {
            // Get name from BotNameGenerator to use as default name
            m_Settings.PlayerName = BotNameGenerator.GetRandomName();
            IsTryingToConnect = false;
            ClientIsInGame = false;
#if UNITY_SERVER && !UNITY_EDITOR
            return;
#endif

            #region ServiceSignin

            m_ProfileService = new PlayerAuthentication();
            await m_ProfileService.SignIn(m_Settings.PlayerName);
            m_Matchmaker = new ClientMatchmaker();
            IsInitialized = true;

            #endregion
        }

        public async Task Matchmake()
        {
            Debug.Log("Beginning Matchmaking.");
            SetIPAndPort(null, 0);
            MatchMakingUI.Instance.UpdateConnectionStatus("[Matchmaker] Searching...");

            if (string.IsNullOrEmpty(Application.cloudProjectId))
            {
                Debug.LogWarning($"To use Unity's dashboard services, " +
                                 "you need to link your Unity project to a project ID. " +
                                 "To do this, go to Project Settings to select your organization, " +
                                 "select your project and then link a project ID. " +
                                 "You also need to make sure your organization has access to the required products. " +
                                 "Visit https://dashboard.unity3d.com to sign up.");

                return;
            }

            try
            {
                var matchmakingResult = await m_Matchmaker.Matchmake(m_ProfileService.LocalPlayer);
                if (matchmakingResult.result == MatchmakerPollingResult.Success)
                {
                    MatchMakingUI.Instance.UpdateConnectionStatus("[Matchmaker] Match found! Queued to join..."); 
                    Debug.Log($"[Matchmaker] Matchmaking Success! Connecting to {IP} : {Port}");
                    await Task.Delay(5000); // Hack: Give the server some time to process before connecting. This should be fixed!
                    HasMatchmakingSuccess = true;
                    MatchMakingUI.Instance.UpdateConnectionStatus($"[Netcode] Connecting to {IP} : {Port}...");
                    ConnectToServer();
                }
                else
                {
                    MatchMakingUI.Instance.UpdateConnectionStatus($"[Matchmaker] {matchmakingResult.result}] - {matchmakingResult.resultMessage}");
                    Debug.LogError($"[Matchmaker] {matchmakingResult.result}] - {matchmakingResult.resultMessage}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Matchmaker] Error Matchmaking: {ex}");
                MatchMakingUI.Instance.UpdateConnectionStatus($"[Matchmaker] Error Matchmaking: {ex}");
            }
        }

        public void OnDisable()
        {
            m_Matchmaker?.Dispose();
        }

        public void SetProfileServiceName(string newValue)
        {
            m_Settings.PlayerName = newValue;
            m_ProfileService.LocalPlayer.SetName(newValue);
        }

        public void UpdateConnectionStatusLabel()
        {
            MatchMakingUI.Instance.UpdateConnectionStatus("[Netcode] Attempting to Connect...");
            Debug.Log($"Attempting to Connect to {IP}:{Port}.");
            IsTryingToConnect = true;
        }

        public void ConnectionSucceeded()
        {
            ClientIsInGame = true;
            MatchMakingUI.Instance.UpdateConnectionStatus("[Netcode] Connected to Server...");
            Debug.Log("Connected to Server...");
            IsTryingToConnect = false;
        }

        public void ConnectionFailed()
        {
            IsTryingToConnect = false;
            ClientIsInGame = false;
            Debug.LogWarning($"Failed to connect");
        }

        public void ConnectToServer()
        {
            ServerConnectionUtils.RequestConnection(IP, Port);
            if (ClientIsInGame)
                return;
            UpdateConnectionStatusLabel();
        }

        public void SetIPAndPort(string ip, ushort port)
        {
            HasMatchmakingSuccess = false;
            IP = ip;
            Port = port;
        }
    }
}