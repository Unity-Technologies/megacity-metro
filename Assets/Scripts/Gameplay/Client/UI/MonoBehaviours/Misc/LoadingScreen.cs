using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.MegacityMetro.UI
{
    /// <summary>
    /// Reads the progress value from GameLoadInfo singleton and update the loading progress bar accordingly
    /// </summary>
    public class LoadingScreen : MonoBehaviour
    {
        public static LoadingScreen Instance { get; private set; }
        
        [HideInInspector] public bool IsVisible;

        private VisualElement m_LoadingScreen;
        private ProgressBar m_ProgressBar;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                SetUpUI();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void SetUpUI()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            m_LoadingScreen = root.Q<VisualElement>("loading-screen");
            m_ProgressBar = root.Q<ProgressBar>("progressbar");
        }

        public void UpdateProgressBar(float progress)
        {
            m_ProgressBar.value = math.lerp(m_ProgressBar.value, progress, Time.deltaTime);
        }

        public void Show()
        {
            // Reset progress
            m_ProgressBar.value = 0;
            m_LoadingScreen.style.display = DisplayStyle.Flex;
            m_LoadingScreen.style.opacity = 1;
            IsVisible = true;
        }

        public void Hide()
        {
            m_LoadingScreen.style.display = DisplayStyle.None;
            IsVisible = false;
        }
    }
}