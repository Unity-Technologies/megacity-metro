using System;
using System.Threading.Tasks;
using UnityEngine;
using Unity.MegacityMetro.Gameplay;
using Unity.MegacityMetro.UGS;
using UnityEngine.UIElements;

namespace Unity.MegacityMetro.UI
{
    public enum MenuState
    {
        MainMenu,
        MultiplayerMenu,
        GameSettings,
        LoadingScreen,
        ModalWindow,
        Matchmaking
    }

    /// <summary>
    /// Manages the UI for the main menu.
    /// This sets the audio settings for the city.
    /// Defines if the player should be manual or automatic.
    /// Allows the execution to exiting by pressing Escape
    /// Has access to the UI game settings
    /// </summary>
    public class MainMenu : MonoBehaviour
    {
        public event Action<GameMode> OnGameModeSelected;
        public event Action OnBackToMainMenu;
        public MenuState CurrentState;
        [SerializeField] private PlayerInfoItemSettings m_PlayerSettings;

        private UIGameSettings m_GameSettings;

        // Base Menu Options
        private VisualElement m_BaseMenuOptions;
        private Button m_SinglePlayerButton;
        private Button m_MultiplayerButton;
        private Button m_QuitButton;
        private Button m_GameSettingsButton;
        private VisualElement m_MainMenuContainer;
        private Task _AutomaticMatchmakingTask; // TODO: This should really be moved into MatchMakingConnector.
        private GameInput.UIActions m_UIActions;
        [SerializeField] private StyleSheet m_MobileStyleSheet;

        public static MainMenu Instance { get; private set; }

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

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                if (m_GameSettingsButton != null)
                    m_GameSettingsButton.clicked -= ShowGameSettings;
                if (m_QuitButton != null)
                    m_QuitButton.clicked -= QuitDemo;
            }
        }

        private void Start()
        {
            if (SceneController.IsReturningToMainMenu)
            {
                SceneController.IsReturningToMainMenu = false;
                ServerConnectionUtils.CreateDefaultWorld();
            }
#if UNITY_SERVER && !UNITY_EDITOR
            Debug.Log("Beginning server mode");
            gameObject.SetActive(false);
#else
            InitUI();
            m_UIActions = new GameInput().UI;
            m_UIActions.Back.performed += _ => OnBackAction();

            m_UIActions.Enable();
#endif
        }

        private void Update()
        {
            PollForAutomaticMatchmakeFlow();
        }

        /// <summary>
        /// Polling to support Automatic flows: This is essentially "Retry until success".
        /// </summary>
        private void PollForAutomaticMatchmakeFlow()
        {
#if !UNITY_SERVER
            if (!CommandLineConfig.AutomaticallyMatchmake) return;
            if (MatchMakingConnector.Instance.ClientIsInGame || MatchMakingConnector.Instance.IsTryingToConnect ||
                !MatchMakingConnector.Instance.IsMatchmakerInitialized) return;
            if (SceneController.IsReturningToMainMenu) return;
            if (_AutomaticMatchmakingTask != null && !_AutomaticMatchmakingTask.IsCompleted) return;
            Debug.Log("[CLI] Detected automatic matchmaking requested! Beginning matchmaking now...");
            OnMultiplayerButtonClicked();
            _AutomaticMatchmakingTask = MultiplayerMenu.Instance.Matchmake();
#endif
        }

        private void InitUI()
        {
            m_PlayerSettings.GameMode = GameMode.None;
            var root = GetComponent<UIDocument>().rootVisualElement;

#if UNITY_ANDROID || UNITY_IPHONE
            root.styleSheets.Clear();
            root.styleSheets.Add(m_MobileStyleSheet);
#endif

            m_MainMenuContainer = root.Q<VisualElement>("main-menu-container");

            // Base Menu Options
            m_BaseMenuOptions = m_MainMenuContainer.Q<VisualElement>("base-menu-options");
            m_GameSettingsButton = m_MainMenuContainer.Q<Button>("settings-button");
            m_QuitButton = m_MainMenuContainer.Q<Button>("quit-button");
            m_SinglePlayerButton = m_MainMenuContainer.Q<Button>("single-player-button");
            m_MultiplayerButton = m_MainMenuContainer.Q<Button>("multiplayer-button");

            m_GameSettingsButton.clicked += ShowGameSettings;
            m_QuitButton.clicked += QuitDemo;
            m_SinglePlayerButton.clicked += OnSinglePlayerButtonClicked;
            m_MultiplayerButton.clicked += OnMultiplayerButtonClicked;

            m_SinglePlayerButton.RegisterCallback<MouseOverEvent>(_ => { m_SinglePlayerButton.Focus(); });
            m_MultiplayerButton.RegisterCallback<MouseOverEvent>(_ => { m_MultiplayerButton.Focus(); });
            m_GameSettingsButton.RegisterCallback<MouseOverEvent>(_ => { m_GameSettingsButton.Focus(); });
            m_QuitButton.RegisterCallback<MouseOverEvent>(_ => { m_QuitButton.Focus(); });
            m_SinglePlayerButton.Focus();

            MatchMakingConnector.Instance.ClientIsInGame = false;
            CursorUtils.ShowCursor();
        }

        private void OnSinglePlayerButtonClicked()
        {
            m_PlayerSettings.GameMode = GameMode.SinglePlayer;
            SceneController.LoadGame();
            Hide();
        }

        private void OnMultiplayerButtonClicked()
        {
            ToggleBaseMenuOptions();
            m_PlayerSettings.GameMode = GameMode.Multiplayer;
            OnGameModeSelected?.Invoke(m_PlayerSettings.GameMode);
            CurrentState = MenuState.MultiplayerMenu;
        }

        private void OnBackAction()
        {
            switch (CurrentState)
            {
                case MenuState.MainMenu:
                    QuitDemo();
                    break;
                case MenuState.MultiplayerMenu:
                    OnBackToMainMenu?.Invoke();
                    CurrentState = MenuState.MainMenu;
                    break;
            }
        }

        private void OnDisable()
        {
            m_UIActions.Disable();
        }

        public void ToggleBaseMenuOptions()
        {
            if (m_BaseMenuOptions.style.display == DisplayStyle.None)
            {
                m_BaseMenuOptions.style.display = DisplayStyle.Flex;
                m_PlayerSettings.GameMode = GameMode.None;
            }
            else
            {
                m_BaseMenuOptions.style.display = DisplayStyle.None;
            }
        }

        public void ConnectionSucceeded()
        {
            Hide();
            MatchMakingConnector.Instance.ConnectionSucceeded();
        }

        public void ConnectionFailed()
        {
            MatchMakingConnector.Instance.ConnectionFailed();
            SceneController.LoadMenu();
        }

        public void Show()
        {
            m_MainMenuContainer.style.display = DisplayStyle.Flex;
            CurrentState = MenuState.MainMenu;
            CursorUtils.ShowCursor();
        }

        private void Hide()
        {
            LoadingScreen.Instance.Show();
            m_MainMenuContainer.style.display = DisplayStyle.None;
            CurrentState = MenuState.LoadingScreen;
        }

        private void QuitDemo()
        {
            ModalWindow.Instance.Show("Are you sure you want to quit?", "Quit", "Cancel",
                () => QuitSystem.WantsToQuit = true);
            CurrentState = MenuState.ModalWindow;
        }

        private void ShowGameSettings()
        {
            if (m_GameSettings == null)
            {
                m_GameSettings = FindObjectOfType<UIGameSettings>();
            }

            m_GameSettings.Show();
            CurrentState = MenuState.GameSettings;
        }
    }
}