using System.Collections.Generic;
using MegacityMetro.CustomUI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.MegacityMetro.UI
{
    /// <summary>
    /// Contains the shared and global properties and Methods for the UI tabs Views in GameSettings.
    /// Manages how to show and hide the states should be controlled.
    /// </summary>
    public abstract class UISettingsTab : MonoBehaviour
    {
        protected bool IsInitialized;
        private string m_ViewName => "game-settings";
        public VisualElement GameSettingsView { get; set; }

        public abstract string TabName { get; }
        private Dictionary<CustomSliderInt, int> m_CurrentSliderIntData = new();
        private Dictionary<CustomSlider, float> m_CurrentSliderData = new();
        private Dictionary<CustomToggle, bool> m_CurrentToggleData = new();
        private Dictionary<CustomSelector, string> m_CurrentSelectorFieldData = new();

        public void Show()
        {
            if (!IsInitialized)
            {
                var root = GetComponent<UIDocument>().rootVisualElement;
                if (root.Q<VisualElement>(m_ViewName).style.display == DisplayStyle.Flex)
                    Initialize();
            }

            SaveCurrentState();
        }

        public void Hide() { }

        public void Apply()
        {
            SaveCurrentState();
        }
        
        protected virtual void SaveCurrentState()
        {
        }

        protected virtual void Initialize()
        {
            IsInitialized = true;
        }

        public virtual void Reset()
        {
        }

        protected void UpdateSliderIntCurrentState(CustomSliderInt slider)
        {
            m_CurrentSliderIntData[slider] = slider.value;
        }
        
        
        protected void ResetSliderIntCurrentState(CustomSliderInt slider)
        {
            if (m_CurrentSliderIntData.TryGetValue(slider, out var value))
            {
                slider.value = value;
            }
        }
        
        protected void UpdateSliderCurrentState(CustomSlider slider)
        {
            m_CurrentSliderData[slider] = slider.value;
        }
        
        protected void ResetSliderCurrentState(CustomSlider slider)
        {
            if (m_CurrentSliderData.TryGetValue(slider, out var value))
            {
                slider.value = value;
            }
        }

        protected void UpdateCurrentToggleState(CustomToggle toggle)
        {
            m_CurrentToggleData[toggle] = toggle.value;
        }

        protected void ResetCurrentToggleState(CustomToggle toggle)
        {
            if (m_CurrentToggleData.TryGetValue(toggle, out var value))
            {
                toggle.value = value;
            }
        }
        
        protected void UpdateCurrentSelectorFieldState(CustomSelector selectorField)
        {
            m_CurrentSelectorFieldData[selectorField] = selectorField.value;
        }

        protected void ResetCurrentSelectorFieldState(CustomSelector selectorField)
        {
            if (m_CurrentSelectorFieldData.TryGetValue(selectorField, out var value))
            {
                selectorField.value = value;
            }
        }
    }
}
