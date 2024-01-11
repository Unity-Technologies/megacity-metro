using Unity.MegacityMetro.CameraManagement;
using Unity.MegacityMetro.Gameplay;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.MegacityMetro.UI
{
    /// <summary>
    /// Game Settings Options
    /// </summary>
    [RequireComponent(typeof(UIGameSettings))]
    public class GameSettingsOptionsMenu : MonoBehaviour
    {
        public static GameSettingsOptionsMenu Instance { get; private set; }

        private UIGameSettings m_UIGameSettings;
        private VisualElement m_SettingsOptions;

        private Button m_TutorialButton;
        private Button m_SettingButton;
        private Button m_ResumeButton;
        private Button m_BackToMenuButton;
        private Button m_QuitButton;

        private bool m_InSettingOptions;
        private GameInput m_OpenOptionsAction;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                m_UIGameSettings = GetComponent<UIGameSettings>();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            m_SettingsOptions = root.Q<VisualElement>("settings-options-panel");
            m_TutorialButton = root.Q<Button>("tutorial-btn");
            m_SettingButton = root.Q<Button>("settings-btn");
            m_ResumeButton = root.Q<Button>("back-to-game-btn");
            m_BackToMenuButton = root.Q<Button>("back-to-menu-btn");
            m_QuitButton = root.Q<Button>("quit-button");

            m_TutorialButton.RegisterCallback<ClickEvent>(_ =>
            {
                TutorialScreen.Instance.ShowTutorial();
                ShowSettingsOptions(false);
            });

            m_ResumeButton.RegisterCallback<ClickEvent>(_ => { ShowSettingsOptions(false); });

            m_BackToMenuButton.RegisterCallback<ClickEvent>(_ =>
            {
                HybridCameraManager.Instance.Reset();
                DisconnectPlayerAndShowMenu();
            });

            m_SettingButton.RegisterCallback<ClickEvent>(_ =>
            {
                if (!m_UIGameSettings.IsVisible)
                {
                    m_UIGameSettings.Show();
                }
                else
                {
                    m_UIGameSettings.Hide();
                }
            });

            m_OpenOptionsAction = new GameInput();
            m_OpenOptionsAction.Enable();
            m_OpenOptionsAction.UI.Enable();

            m_QuitButton.clicked += () =>
            {
                ModalWindow.Instance.Show("Are you sure you want to quit?", "Confirm", "Cancel",
                    () => { QuitSystem.WantsToQuit = true; });
            };
        }

        private void DisconnectPlayerAndShowMenu()
        {
            ShowSettingsOptions(false);

            QuitSystem.DisconnectAllPlayers();
            if (VivoxManager.Instance != null)
                VivoxManager.Instance.Logout();

            if (PlayerInfoController.Instance.IsSinglePlayer)
                SceneController.LoadMenu();
        }

        public void ShowSettingsOptions(bool display)
        {
            if (SceneController.IsFrontEnd || display == m_InSettingOptions)
                return;

            m_InSettingOptions = display;
            m_SettingsOptions.style.display = display ? DisplayStyle.Flex : DisplayStyle.None;
            m_TutorialButton.SetEnabled(true);

            if (display)
                CursorUtils.ShowCursor();
            else
                CursorUtils.HideCursor();
        }

        private void Update()
        {
            if ((m_OpenOptionsAction.UI.OpenOptions.triggered || m_OpenOptionsAction.UI.Back.triggered) &&
                !m_UIGameSettings.IsVisible)
            {
                ShowSettingsOptions(!m_InSettingOptions);
            }
        }
    }
}