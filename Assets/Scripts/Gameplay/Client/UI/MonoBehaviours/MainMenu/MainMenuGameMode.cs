using Unity.MegacityMetro.Gameplay;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.MegacityMetro.UI
{
    public abstract class MainMenuGameMode : MonoBehaviour
    {
        protected abstract GameMode GameMode { get; }
        protected abstract VisualElement m_MenuOptions { get; }
        protected abstract VisualElement m_AutomaticFocusElement { get; }
        // Reference to main menu
        private MainMenu m_MainMenu;
        
        private void Start()
        {
#if UNITY_SERVER && !UNITY_EDITOR
            gameObject.SetActive(false);
#endif
            InitUI();
        }

        public virtual void InitUI()
        {
            m_MainMenu = GetComponent<MainMenu>();
            m_MainMenu.OnGameModeSelected += OnSelectedMode;
            m_MainMenu.OnBackToMainMenu += BackToTheMenu;
        }

        public void ToggleVisibility()
        {
            if (m_MenuOptions.style.display == DisplayStyle.None)
            {
                m_MenuOptions.style.display = DisplayStyle.Flex;
                m_AutomaticFocusElement.RegisterCallback<GeometryChangedEvent>(_ => m_AutomaticFocusElement.Focus());
            }
            else
            {
                m_MenuOptions.style.display = DisplayStyle.None;
            }
        }
        
        private void OnDestroy()
        {
            if(m_MainMenu != null)
                m_MainMenu.OnGameModeSelected -= OnSelectedMode;
        }
        
        private void OnSelectedMode(GameMode gameMode)
        {
            if(gameMode == GameMode)
                ToggleVisibility();
        }

        protected virtual void BackToTheMenu()
        {
            if(m_MenuOptions.style.display == DisplayStyle.None)
                return;
            
            MatchMakingUI.Instance.SetUIConnectionStatusEnable(false);
            m_MainMenu.ToggleBaseMenuOptions();
            ToggleVisibility();
        }
    }
}