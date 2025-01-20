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
        private GameInput m_GameInput;

        private VisualElement m_Root;
        private FocusController m_FocusController;

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
            m_Root = GetComponent<UIDocument>().rootVisualElement;

            m_FocusController = m_Root.focusController;

            m_SettingsOptions = m_Root.Q<VisualElement>("settings-options-panel");

            m_TutorialButton = m_Root.Q<Button>("tutorial-btn");
            m_SettingButton = m_Root.Q<Button>("settings-btn");
            m_ResumeButton = m_Root.Q<Button>("back-to-game-btn");
            m_BackToMenuButton = m_Root.Q<Button>("back-to-menu-btn");
            m_QuitButton = m_Root.Q<Button>("quit-button");

            m_TutorialButton.clicked += () =>
            {
                TutorialScreen.Instance.ShowTutorial();
                ShowSettingsOptions(false);
            };

            m_ResumeButton.clicked += () => { ShowSettingsOptions(false); };

            m_BackToMenuButton.clicked += () =>
            {
                HybridCameraManager.Instance.Reset();
                DisconnectPlayerAndShowMenu();
            };

            m_SettingButton.clicked += () =>
            {
                m_SettingsOptions.style.display = DisplayStyle.None;
                m_UIGameSettings.Show(() => { m_SettingsOptions.style.display = DisplayStyle.Flex; });
            };

            m_GameInput = new GameInput();
            m_GameInput.UI.Enable();

            m_QuitButton.clicked += () =>
            {
                m_SettingsOptions.style.display = DisplayStyle.None;
                ModalWindow.Instance.Show("Are you sure you want to quit?", "Confirm", "Cancel",
                    OnQuit, OnCancelQuit);
            };
        }

        private void OnCancelQuit()
        {
            m_SettingsOptions.style.display = DisplayStyle.Flex;
        }

        private void OnQuit()
        {
            m_SettingsOptions.style.display = DisplayStyle.Flex;
            QuitSystem.WantsToQuit = true;
        }

        private void DisconnectPlayerAndShowMenu()
        {
            ShowSettingsOptions(false);

            QuitSystem.DisconnectAllPlayers();

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
            {
                CursorUtils.ShowCursor();
                UIEvents.OnPauseOptionsShown?.Invoke(true);
            }
            else
            {
                CursorUtils.HideCursor();
                UIEvents.OnPauseOptionsShown?.Invoke(false);
            }
        }

        private void Update()
        {
            if (m_FocusController.focusedElement == null && m_InSettingOptions && !m_UIGameSettings.IsVisible)
            {
                m_ResumeButton.Focus();
            }
#if UNITY_SWITCH 
            if (m_GameInput.UI.OpenOptions.triggered &&!m_UIGameSettings.IsVisible)
            {
                ShowSettingsOptions(!m_InSettingOptions);
            }
#else
            if ((m_GameInput.UI.OpenOptions.triggered || m_GameInput.UI.Back.triggered) &&
                !m_UIGameSettings.IsVisible)
            {
                ShowSettingsOptions(!m_InSettingOptions);
            }
#endif
        }
    }
}