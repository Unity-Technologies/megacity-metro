using Unity.NAME.Game;
using Unity.NAME.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Unity.NAME.UI
{
    // This is the component that handles the display of the InGameMenu (TAB or P at runtime)

    public class InGameMenuManager : MonoBehaviour
    {
        [Tooltip("Root GameObject of the menu used to toggle its activation")]
        public GameObject MenuRoot;
        [Tooltip("Master volume when menu is open")]
        [Range(0.001f, 1f)]
        public float VolumeWhenMenuOpen = 0.5f;
        [Tooltip("Toggle component for shadows")]
        public Toggle ShadowsToggle;
        [Tooltip("Toggle component for framerate display")]
        public Toggle FramerateToggle;
        [Tooltip("GameObject for the controls")]
        public GameObject ControlImage;

        FramerateCounter m_FramerateCounter;
        GameInputActions m_InputActions;
        //bool m_DisplayMenu = false;

        void Awake()
        {
            m_InputActions = new GameInputActions();
            m_InputActions.UI.DisplayMenu.started += ctx => TogglePauseMenu();
            m_InputActions.UI.Cancel.started += ctx => ClosePauseMenu();
        }

        void OnEnable()
        {
            m_InputActions?.Enable();
        }

        void Start()
        {
            m_FramerateCounter = FindObjectOfType<FramerateCounter>();
            DebugUtils.HandleErrorIfNullFindObject<FramerateCounter, InGameMenuManager>(m_FramerateCounter, this);

            MenuRoot.SetActive(false);

            ShadowsToggle.isOn = QualitySettings.shadows != ShadowQuality.Disable;
            ShadowsToggle.onValueChanged.AddListener(OnShadowsChanged);

            FramerateToggle.isOn = m_FramerateCounter.UiText.gameObject.activeSelf;
            FramerateToggle.onValueChanged.AddListener(OnFramerateCounterChanged);
        }

        public void ClosePauseMenu()
        {
            SetPauseMenuActivation(false);
        }

        public void TogglePauseMenu()
        {
            SetPauseMenuActivation(!MenuRoot.activeSelf);
        }

        void SetPauseMenuActivation(bool active)
        {
            MenuRoot.SetActive(active);
            ControlImage.SetActive(false);  // Reset no matter what

            if (MenuRoot.activeSelf)
            {
                //     Cursor.lockState = CursorLockMode.None;
                //  Cursor.visible = true;
                Time.timeScale = 0f;
                AudioUtility.SetMasterVolume(VolumeWhenMenuOpen);

                EventSystem.current.SetSelectedGameObject(null);
            }
            else
            {
                //   Cursor.lockState = CursorLockMode.Locked;
                //   Cursor.visible = false;
                Time.timeScale = 1f;
                AudioUtility.SetMasterVolume(1);
            }
        }

        void OnShadowsChanged(bool newValue)
        {
            QualitySettings.shadows = newValue ? ShadowQuality.All : ShadowQuality.Disable;
        }

        void OnFramerateCounterChanged(bool newValue)
        {
            m_FramerateCounter.UiText.gameObject.SetActive(newValue);
        }

        public void OnShowControlButtonClicked(bool show)
        {
            ControlImage.SetActive(show);
            if (show)
            {
                Button button = ControlImage.GetComponentInChildren<Button>();
                EventSystem.current.SetSelectedGameObject(button != null ? button.gameObject : null);
            }
            else
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        void OnDisable()
        {
            m_InputActions?.Disable();
        }
    }
}
