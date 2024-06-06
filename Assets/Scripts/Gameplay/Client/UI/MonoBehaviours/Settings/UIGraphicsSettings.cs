using MegacityMetro.CustomUI;
using Unity.MegacityMetro.Utils;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace Unity.MegacityMetro.UI
{
    /// <summary>
    /// Access to GameObjects in the Scene and graphics settings that allows change the quality of the game.
    /// Uses the Graphic Settings View to modify by Toggle and DropdownField that should be Modify in the UI,
    /// These controls modify:
    ///  - Postprocessing
    ///  - Quality Settings
    ///  - Texture Detail
    ///  - Level of Detail
    ///  - VSync
    ///  - ScreenMode
    /// </summary>
    public class UIGraphicsSettings : UISettingsTab
    {
        [SerializeField] private Volume m_PostProcessing;
        [SerializeField] private UIScreenResolution ResolutionScreen;

        private CustomToggle m_PostProcessingValue;
        private CustomToggle m_VerticalSyncValue;

        private CustomSelector m_QualityValue;
        private CustomSelector m_ScreenModeValue;
        private CustomSelector m_ScreenResolution;
        private CustomSelector m_TextureDetailsValue;

        public override string TabName => "graphics-settings";
        private bool m_CanSetAsCustom = true;

        private void Start()
        {
            Initialize();
        }

        protected override void Initialize()
        {
            base.Initialize();

            var root = GetComponent<UIDocument>().rootVisualElement;
            m_PostProcessingValue = root.Q<CustomToggle>("postprocessing");
            m_VerticalSyncValue = root.Q<CustomToggle>("vertical-sync");
            m_QualityValue = root.Q<CustomSelector>("quality-settings");
            m_ScreenResolution = root.Q<CustomSelector>("screen-resolution");
            m_ScreenModeValue = root.Q<CustomSelector>("screen-mode");
            m_TextureDetailsValue = root.Q<CustomSelector>("texture-details");

            m_ScreenModeValue.choices = ResolutionScreen.GetResolutionModes();
            m_ScreenResolution.choices = ResolutionScreen.GetResolutionOptions();
            m_PostProcessingValue.RegisterValueChangedCallback(OnPostprocessingChanged);
            m_VerticalSyncValue.RegisterValueChangedCallback(OnVsyncChanged);
            m_QualityValue.RegisterValueChangedCallback(OnGraphicsQualityChanged);
            m_TextureDetailsValue.RegisterValueChangedCallback(OnTextureDetailsChanged);
            
             switch (QualitySettings.GetQualityLevel())
            {
                case 0:
                    m_QualityValue.value = m_QualityValue.choices[0];
                    OnLowButtonOnClicked();
                    break;
                case 1:
                    m_QualityValue.value = m_QualityValue.choices[1];
                    OnMediumButtonOnClicked();
                    break;
                case 2:
                    m_QualityValue.value = m_QualityValue.choices[2];
                    OnHighButtonOnClicked();
                    break;
            }

            CheckSavedData();
            
            m_QualityValue.RegisterCallback<GeometryChangedEvent>(_ => m_QualityValue.Focus());
        }
        
        private void CheckSavedData()
        {
            var graphicsSettingsData = PersistentDataManager.Instance.GetGraphicsSettings();

            m_QualityValue.value = m_QualityValue.choices[graphicsSettingsData.QualityLevelIndex];
            m_ScreenModeValue.value = m_ScreenModeValue.choices[graphicsSettingsData.ScreenModeIndex];
            m_ScreenResolution.value = m_ScreenResolution.choices[graphicsSettingsData.ScreenResolutionIndex];
            m_TextureDetailsValue.value = m_TextureDetailsValue.choices[graphicsSettingsData.TextureDetailIndex];
            m_PostProcessingValue.value = graphicsSettingsData.PostProcessingEnabled;
#if !(UNITY_ANDROID || UNITY_IPHONE)
            m_VerticalSyncValue.value = graphicsSettingsData.VSyncEnabled;
#endif

            // Force set initial values
            QualitySettings.vSyncCount = m_VerticalSyncValue.value ? 1 : 0;
            m_PostProcessing.enabled = m_PostProcessingValue.value;
            SetTextureDetail(m_TextureDetailsValue.value.ToLower());
            ResolutionScreen.SetScreenMode(m_ScreenModeValue.value.ToLower());
            ResolutionScreen.SetResolution(m_ScreenResolution.value.ToLower());
        }

        protected override void SaveCurrentState()
        {
            base.SaveCurrentState();

            UpdateCurrentToggleState(m_PostProcessingValue);
            UpdateCurrentToggleState(m_VerticalSyncValue);
            UpdateCurrentSelectorFieldState(m_QualityValue);
            UpdateCurrentSelectorFieldState(m_ScreenModeValue);
            UpdateCurrentSelectorFieldState(m_TextureDetailsValue);
            
#if !(UNITY_ANDROID || UNITY_IPHONE)
            ResolutionScreen.SetResolution(m_ScreenResolution.value.ToLower());
            ResolutionScreen.SetScreenMode(m_ScreenModeValue.value.ToLower());
#endif

            SaveData();
        }

        private void SaveData()
        {
            var graphicsSettingsData = new GraphicsSettingsData
            {
                QualityLevelIndex = m_QualityValue.index,
                ScreenModeIndex = m_ScreenModeValue.index,
                ScreenResolutionIndex = m_ScreenResolution.index,
                TextureDetailIndex = m_TextureDetailsValue.index,
                PostProcessingEnabled = m_PostProcessingValue.value,
                VSyncEnabled = m_VerticalSyncValue.value
            };

            PersistentDataManager.Instance.SaveGraphicsSettings(graphicsSettingsData);
        }

        public override void Reset()
        {
            base.Reset();

            ResetCurrentToggleState(m_PostProcessingValue);
            ResetCurrentToggleState(m_VerticalSyncValue);
            ResetCurrentSelectorFieldState(m_QualityValue);
            ResetCurrentSelectorFieldState(m_ScreenModeValue);
            ResetCurrentSelectorFieldState(m_TextureDetailsValue);
        }

        private void OnHighButtonOnClicked()
        {
            m_CanSetAsCustom = false;
            QualitySettings.SetQualityLevel(2);
#if !(UNITY_ANDROID || UNITY_IPHONE)
            m_VerticalSyncValue.value = true;
#endif
            m_PostProcessingValue.value = true;
            m_TextureDetailsValue.value = "High";
        }

        private void OnMediumButtonOnClicked()
        {
            m_CanSetAsCustom = false;
            QualitySettings.SetQualityLevel(1);
#if !(UNITY_ANDROID || UNITY_IPHONE)
            m_VerticalSyncValue.value = false;
#endif
            m_PostProcessingValue.value = true;
            m_TextureDetailsValue.value = "Medium";
        }

        private void OnLowButtonOnClicked()
        {
            m_CanSetAsCustom = false;
            QualitySettings.SetQualityLevel(0);
#if !(UNITY_ANDROID || UNITY_IPHONE)
            m_VerticalSyncValue.value = false;
#endif
            m_PostProcessingValue.value = false;
            m_TextureDetailsValue.value = "Low";
        }

        private void OnDestroy()
        {
            if (IsInitialized)
            {
                m_PostProcessingValue.UnregisterValueChangedCallback(OnPostprocessingChanged);
                m_VerticalSyncValue.UnregisterValueChangedCallback(OnVsyncChanged);
                m_ScreenModeValue.UnregisterValueChangedCallback(OnScreenModeChanged);
                m_TextureDetailsValue.UnregisterValueChangedCallback(OnTextureDetailsChanged);
            }
        }

        private void OnVsyncChanged(ChangeEvent<bool> value)
        {
            QualitySettings.vSyncCount = value.newValue ? 1 : 0;
            SetCustom();
        }

        private void OnPostprocessingChanged(ChangeEvent<bool> value)
        {
            m_PostProcessing.enabled = value.newValue;
            SetCustom();
        }

        private void OnTextureDetailsChanged(ChangeEvent<string> value)
        {
            SetTextureDetail(value.newValue.ToLower());
            SetCustom();
        }

        private void OnGraphicsQualityChanged(ChangeEvent<string> value)
        {
            switch (value.newValue)
            {
                case "High":
                    OnHighButtonOnClicked();
                    break;
                case "Medium":
                    OnMediumButtonOnClicked();
                    break;
                case "Low":
                    OnLowButtonOnClicked();
                    break;
            }
        }

        private void OnScreenModeChanged(ChangeEvent<string> value)
        {
            ResolutionScreen.SetScreenMode(value.newValue.ToLower());
            SetCustom();
        }

        private void SetTextureDetail(string value)
        {
            switch (value)
            {
                case "low":
                    QualitySettings.globalTextureMipmapLimit = 2;
                    MaterialQuality.Low.SetGlobalShaderKeywords();
                    break;
                case "medium":
                    QualitySettings.globalTextureMipmapLimit = 1;
#if UNITY_IPHONE || UNITY_STANDALONE_OSX
                    MaterialQuality.Low.SetGlobalShaderKeywords();
#else
                    MaterialQuality.Medium.SetGlobalShaderKeywords();
#endif     
                    break;
                case "high":
                    QualitySettings.globalTextureMipmapLimit = 0;
#if UNITY_IPHONE || UNITY_STANDALONE_OSX
                    MaterialQuality.Low.SetGlobalShaderKeywords();
#else
                    MaterialQuality.High.SetGlobalShaderKeywords();
#endif
                    break;
            }
        }

        private void SetCustom()
        {
            if (m_CanSetAsCustom)
                m_QualityValue.value = m_QualityValue.choices[3];
        }
    }
}