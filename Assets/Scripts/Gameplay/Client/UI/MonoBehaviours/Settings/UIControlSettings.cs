using MegacityMetro.CustomUI;
using Unity.MegacityMetro.Utils;
using UnityEngine.UIElements;

namespace Unity.MegacityMetro.UI
{
    /// <summary>
    /// This class manages the Controls View in UI Game Settings view.
    /// </summary>
    public class UIControlSettings : UISettingsTab
    {
        public static UIControlSettings Instance { get; private set; }
        
        public override string TabName => "controls-settings";

        private SliderInt m_AimSensitivitySlider;
        private SliderInt m_MouseSensitivitySlider;
        private CustomToggle m_InverseLookHorizontalToggle;
        private CustomToggle m_InverseLookVerticalToggle;

        public float AimAssistanceSensitivity = 1f;
        public float MouseSensitivity = 1f;
        public bool InverseLookHorizontal;
        public bool InverseLookVertical;
        public bool ShouldUpdate;

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

        private void Start()
        {
            Initialize();
        }

        protected override void Initialize()
        {
            base.Initialize();

            var root = GetComponent<UIDocument>().rootVisualElement;
            m_MouseSensitivitySlider = root.Q<SliderInt>("mouse-sensitivity");
            m_AimSensitivitySlider = root.Q<SliderInt>("aim-sensitivity");
            m_InverseLookHorizontalToggle = root.Q<CustomToggle>("invert-look-horizontal");
            m_InverseLookVerticalToggle = root.Q<CustomToggle>("invert-look-vertical");

            m_MouseSensitivitySlider.RegisterValueChangedCallback(OnMouseSensitivityUpdated);
            m_AimSensitivitySlider.RegisterValueChangedCallback(OnAimSensitivityUpdated);
            m_InverseLookHorizontalToggle.RegisterValueChangedCallback(OnInverseLookHorizontalChanged);
            m_InverseLookVerticalToggle.RegisterValueChangedCallback(OnInverseLookVerticalChanged);
            
            CheckSavedData();
        }

        private void OnInverseLookHorizontalChanged(ChangeEvent<bool> evt)
        {
            InverseLookHorizontal = evt.newValue;
        }

        private void OnInverseLookVerticalChanged(ChangeEvent<bool> evt)
        {
            InverseLookVertical = evt.newValue;
        }

        private void OnMouseSensitivityUpdated(ChangeEvent<int> evt)
        {
            MouseSensitivity = evt.newValue / 100f;
        }
        
        private void OnAimSensitivityUpdated(ChangeEvent<int> evt)
        {
            AimAssistanceSensitivity = evt.newValue/ 100f;
        }

        protected override void SaveCurrentState()
        {
            base.SaveCurrentState();
            UpdateSliderCurrentState(m_MouseSensitivitySlider);
            UpdateCurrentToggleState(m_InverseLookHorizontalToggle);
            UpdateCurrentToggleState(m_InverseLookVerticalToggle);
            
            // Tell system to update values
            ShouldUpdate = true;
            
            SaveData();
        }
        
        private void SaveData()
        {
            var controlSettingsData = new ControlSettingsData
            {
                MouseSensitivity = MouseSensitivity,
                InverseLookHorizontal = InverseLookHorizontal,
                InverseLookVertical = InverseLookVertical,
                AimAssistanceSensitivity = AimAssistanceSensitivity
            };
            
            PersistentDataManager.Instance.SaveControlSettings(controlSettingsData);
        }
        
        private void CheckSavedData()
        {
            var controlSettingsData = PersistentDataManager.Instance.GetControlSettings();
            
            m_MouseSensitivitySlider.value = (int) (controlSettingsData.MouseSensitivity * 100f);
            m_AimSensitivitySlider.value = (int) (controlSettingsData.AimAssistanceSensitivity * 100f);
            m_InverseLookHorizontalToggle.value = controlSettingsData.InverseLookHorizontal;
            m_InverseLookVerticalToggle.value = controlSettingsData.InverseLookVertical;
        }

        private void OnDestroy()
        {
            if (IsInitialized)
            {
                m_MouseSensitivitySlider.UnregisterValueChangedCallback(OnMouseSensitivityUpdated);
                m_InverseLookHorizontalToggle.UnregisterValueChangedCallback(OnInverseLookHorizontalChanged);
                m_InverseLookVerticalToggle.UnregisterValueChangedCallback(OnInverseLookVerticalChanged);
            }
        }

        public override void Reset()
        {
            base.Reset();
            ResetSliderCurrentState(m_MouseSensitivitySlider);
            ResetCurrentToggleState(m_InverseLookHorizontalToggle);
            ResetCurrentToggleState(m_InverseLookVerticalToggle);
        }
    }
}