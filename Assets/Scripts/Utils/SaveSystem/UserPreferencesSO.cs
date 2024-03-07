using System;
using UnityEngine;

namespace Unity.MegacityMetro.Utils
{
    [Serializable]
    public struct UserSettingsData
    {
        public GraphicsSettingsData GraphicsSettings;
        public AudioSettingsData AudioSettings;
        public ControlSettingsData ControlSettings;
    }

    [Serializable]
    public struct GraphicsSettingsData
    {
        public int QualityLevelIndex;
        public int ScreenModeIndex;
        public int ScreenResolutionIndex;
        public int TextureDetailIndex;
        public bool PostProcessingEnabled;
        public bool VSyncEnabled;
    }

    [Serializable]
    public struct AudioSettingsData
    {
        public float MasterVolume;
        public float VisualFxVolume;
        public float MusicVolume;
        public float VivoxSpeakerVolume;
        public float VivoxMicrophoneVolume;
    }

    [Serializable]
    public struct ControlSettingsData
    {
        public float MouseSensitivity;
        public float AimAssistanceSensitivity;
        public bool InverseLookHorizontal;
        public bool InverseLookVertical;
    }

    //[CreateAssetMenu(fileName = "UserPreferences", menuName = "Settings/UserSettings", order = 0)]
    public class UserPreferencesSO : ScriptableObject
    {
        public UserSettingsData UserSettings;
    }
}