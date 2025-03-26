using System;
using Unity.MegacityMetro.Gameplay;
using Unity.MegacityMetro.UGS;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace Unity.MegacityMetro.UI
{
    public class MatchMakingUI : MonoBehaviour
    {
        [SerializeField]
        private MultiplayerServerSettings m_ServerSettings;
        public float ConnectionTimeout => m_ServerSettings.ConnectionTimeout;

        // UI Elements

        private DropdownField m_MultiplayerServerDropdownMenu;
        private Label m_ConnectionStatusLabel;
        private VisualElement m_MatchmakingLoadingBar;
        private VisualElement m_MatchmakingSpinner;
        private Button m_MultiplayerPlayButton;
        private TextField m_NameTextField;
        private Button m_MultiplayerReturnButton;
        private VisualElement m_MultiplayerModeGroup;

        public static MatchMakingUI Instance { get; private set; }
        
        public event Action Connecting;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void Init()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            m_ConnectionStatusLabel = root.Q<Label>("connection-label");
            m_MatchmakingSpinner = root.Q<VisualElement>("matchmaking-spinner");
            m_MatchmakingLoadingBar = root.Q<VisualElement>("matchmaking-loading-bar");
            m_MultiplayerServerDropdownMenu = root.Q<DropdownField>("multiplayer-server-location");
            m_MultiplayerServerDropdownMenu.choices = m_ServerSettings.GetUIChoices();
            
            var multiplayerMenuOptions = root.Q<VisualElement>("multiplayer-menu-options");
            m_NameTextField = multiplayerMenuOptions.Q<TextField>("name-textfield");
            m_MultiplayerPlayButton = multiplayerMenuOptions.Q<Button>("multiplayer-play-button");
            m_MultiplayerReturnButton = multiplayerMenuOptions.Q<Button>("multiplayer-return-button");
            m_MultiplayerModeGroup = multiplayerMenuOptions.Q<VisualElement>("multiplayer-mode");
            
            m_MatchmakingLoadingBar.AddToClassList(UIConstants.k_HiddenUssElementClass);
        }

        public bool TryUpdateIPAndPort(string currentIP, out string ip, out ushort port)
        {
            ip = "";
            port = 0;
            if (ServerConnectionUtils.TryParseDomain(currentIP, out var domainIP))
            {
                currentIP = $"{domainIP}:{MatchMakingConnector.Instance.Port}";
            }

            var ipSplit = currentIP.Split(":");
            if (ipSplit.Length < 2)
                return false;

            ip = ipSplit[0];
            var portString = ipSplit[1];
            if (!ushort.TryParse(portString, out port))
                return false;

            return true;
        }

        public void UpdateConnectionStatus(string status)
        {
            m_ConnectionStatusLabel.RemoveFromClassList(UIConstants.k_HiddenUssElementClass);
            m_ConnectionStatusLabel.text = status;
        }

        public void SetUIConnectionStatusEnable(bool matchmaking)
        {
            m_MatchmakingLoadingBar.EnableInClassList(UIConstants.k_HiddenUssElementClass, !matchmaking);
            m_ConnectionStatusLabel.EnableInClassList(UIConstants.k_HiddenUssElementClass, !matchmaking);

            if (matchmaking)
                AnimateLoadingBar();
        }

        public void SetConnectionMode(bool isMatchMaking)
        {
            m_MultiplayerServerDropdownMenu.EnableInClassList(UIConstants.k_HiddenUssElementClass, isMatchMaking);
            
            if (isMatchMaking)
            {
                m_MultiplayerPlayButton.clicked += OnPlaySelected;
            }
            else
            {
                m_MultiplayerServerDropdownMenu.value = m_MultiplayerServerDropdownMenu.choices[0] ?? "LOCAL";
                m_MultiplayerPlayButton.clicked -= OnPlaySelected;
            }
        }

        public async System.Threading.Tasks.Task Matchmake()
        {
            while (true)
            {
                if (MatchMakingConnector.Instance.IsInitialized)
                {
                    MatchMakingConnector.Instance.SetProfileServiceName(m_NameTextField.text);
                    SetUIMatchmaking(true);
                    await MatchMakingConnector.Instance.Matchmake();
                    SetUIMatchmaking(false);
                }
                else
                {
                    UpdateConnectionStatus("Reconnecting...");
                    await MatchMakingConnector.Instance.Reconnect();
                    continue;
                }

                break;
            }
        }
        
        private void OnPlaySelected()
        {
            if (!m_MultiplayerPlayButton.enabledSelf)
                return;

            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                UpdateConnectionStatus("Error: No Internet Connection!");
                ModalWindow.Instance.Show("Error: No Internet Connection!", "OK");
                return;
            }

            if (MatchMakingConnector.Instance.ClientIsInGame)
            {
                Debug.LogWarning("Cant hit play while already in-game!");
                return;
            }

            Connecting?.Invoke();

            _ = Matchmake();
        }

        private void SetUIMatchmaking(bool matchmaking)
        {
            m_NameTextField.EnableInClassList(UIConstants.k_HiddenUssElementClass, matchmaking);
            m_MultiplayerModeGroup.EnableInClassList(UIConstants.k_HiddenUssElementClass, matchmaking);
            m_MultiplayerPlayButton.EnableInClassList(UIConstants.k_HiddenUssElementClass, matchmaking);
            m_MultiplayerReturnButton.EnableInClassList(UIConstants.k_HiddenUssElementClass, matchmaking);

            // Show when is doing matchmaking
            SetUIConnectionStatusEnable(matchmaking);
            m_MultiplayerPlayButton.SetEnabled(!matchmaking);
            m_MultiplayerReturnButton.SetEnabled(!matchmaking);
        }

        private void AnimateLoadingBar()
        {
            if (!m_MatchmakingLoadingBar.ClassListContains(UIConstants.k_HiddenUssElementClass))
            {
                var currentRotation = m_MatchmakingSpinner.style.rotate;
                var angle = currentRotation.value.angle.value;
                var q = Quaternion.Euler(0f, 0f, angle + 180f);
                m_MatchmakingSpinner.experimental.animation.Rotation(q, 500).OnCompleted(AnimateLoadingBar)
                    .easingCurve = Easing.Linear;
            }
        }
    }
}
