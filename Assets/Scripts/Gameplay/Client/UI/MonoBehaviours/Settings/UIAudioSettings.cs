using MegacityMetro.CustomUI;
using Unity.MegacityMetro.Audio;
using Unity.MegacityMetro.Gameplay;
using Unity.MegacityMetro.Utils;
using UnityEngine;
using UnityEngine.UIElements;
using System.Runtime.InteropServices;

namespace Unity.MegacityMetro.UI
{
    /// <summary>
    /// Modifies the AudioMixer Groups via AudioMaster
    /// Reads the data to set the Sliders in the AudioView
    /// </summary>
    public class UIAudioSettings : UISettingsTab
    {
        private CustomSliderInt m_VolumeSlider;
        private CustomSliderInt m_SoundFXSlider;
        private CustomSliderInt m_MusicSlider;
        private CustomSliderInt m_VivoxVolumeSlider;
        private CustomSliderInt m_VivoxMicrophoneSlider;

        private float m_MaxVolume;
        private float m_MaxSoundFX;
        private float m_MaxMusic;

        private float m_CachedMediaVolume = float.MaxValue;

        private const float k_MinVolume = 0.01f;
        
        public override string TabName => "audio-settings";

        private void Start()
        {
            Initialize();
        }

        private void CheckSavedData()
        {
#if !UNITY_SWITCH            
            var audioSettingsData = PersistentDataManager.Instance.GetAudioSettings();
            m_VolumeSlider.value = (int) (audioSettingsData.MasterVolume * 100f);
            m_SoundFXSlider.value = (int) (audioSettingsData.VisualFxVolume * 100f);
            m_MusicSlider.value = (int) (audioSettingsData.MusicVolume * 100f);
            m_VivoxVolumeSlider.value = (int) (audioSettingsData.VivoxSpeakerVolume * 100f);
            m_VivoxMicrophoneSlider.value = (int) (audioSettingsData.VivoxMicrophoneVolume * 100f);
#endif            
            // Initialize the AudioMixer with the saved data
            SetMasterVolume(Mathf.Log10((m_VolumeSlider.value + k_MinVolume) / 100f) * 20f + m_MaxVolume);
            AudioMaster.Instance.soundFX.audioMixer.SetFloat("sound-fx", Mathf.Log10((m_SoundFXSlider.value + k_MinVolume) / 100f) * 20 + m_MaxSoundFX);
            AudioMaster.Instance.music.audioMixer.SetFloat("music", Mathf.Log10((m_MusicSlider.value + k_MinVolume) / 100f) * 20 + m_MaxMusic);
            SetSpeakerVolume(m_VivoxVolumeSlider.value);
            SetMicrophoneVolume(m_VivoxMicrophoneSlider.value);
        }

        protected override void Initialize()
        {
            base.Initialize();

            var root = GetComponent<UIDocument>().rootVisualElement;
            m_VolumeSlider = root.Q<CustomSliderInt>("volume");
            m_SoundFXSlider = root.Q<CustomSliderInt>("sound-fx");
            m_MusicSlider = root.Q<CustomSliderInt>("music");
            m_VivoxVolumeSlider = root.Q<CustomSliderInt>("vivox-volume");
            m_VivoxMicrophoneSlider = root.Q<CustomSliderInt>("vivox-microphone-volume");

            // Set the max values for the sliders
#if UNITY_ANDROID
            AudioMaster.Instance.OnDeviceVolumeReceived += OnDeviceVolumeReceived;
#endif
            AudioMaster.Instance.volume.audioMixer.GetFloat("volume", out m_MaxVolume);
            AudioMaster.Instance.soundFX.audioMixer.GetFloat("sound-fx", out m_MaxSoundFX);
            AudioMaster.Instance.music.audioMixer.GetFloat("music", out m_MaxMusic);

            m_VivoxVolumeSlider.RegisterValueChangedCallback(OnVivoxVolumeUpdated);
            m_VivoxMicrophoneSlider.RegisterValueChangedCallback(OnVivoxMicrophoneVolumeUpdated);
            m_SoundFXSlider.RegisterValueChangedCallback(OnSoundFXUpdated);
            m_VolumeSlider.RegisterValueChangedCallback(OnMasterVolumeUpdated);
            m_MusicSlider.RegisterValueChangedCallback(OnMusicUpdated);

            if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64 &&
                Application.platform == RuntimePlatform.OSXPlayer)
            {
                m_VivoxVolumeSlider.style.display = VivoxManager.Instance == null? DisplayStyle.None: DisplayStyle.Flex;
                m_VivoxMicrophoneSlider.style.display = VivoxManager.Instance == null? DisplayStyle.None: DisplayStyle.Flex;
            }
            
            CheckSavedData();
            
            m_VolumeSlider.RegisterCallback<GeometryChangedEvent>(_ => m_VolumeSlider.Focus());
        }
        
        protected override void SaveCurrentState()
        {
            base.SaveCurrentState();
            UpdateSliderIntCurrentState(m_VolumeSlider);
            UpdateSliderIntCurrentState(m_SoundFXSlider);
            UpdateSliderIntCurrentState(m_VivoxVolumeSlider);
            UpdateSliderIntCurrentState(m_VivoxMicrophoneSlider);

            SaveData();
        }

        private void SaveData()
        {
            var audioSettingsData = new AudioSettingsData
            {
                MasterVolume = m_VolumeSlider.value,
                VisualFxVolume = m_SoundFXSlider.value,
                MusicVolume = m_MusicSlider.value,
                VivoxSpeakerVolume = m_VivoxVolumeSlider.value,
                VivoxMicrophoneVolume = m_VivoxMicrophoneSlider.value
            };
#if !UNITY_SWITCH
            PersistentDataManager.Instance.SaveAudioSettings(audioSettingsData);
#endif
        }

        private void OnDestroy()
        {
            if (IsInitialized)
            {
                m_SoundFXSlider.UnregisterValueChangedCallback(OnSoundFXUpdated);
                m_VolumeSlider.UnregisterValueChangedCallback(OnMasterVolumeUpdated);
                m_MusicSlider.UnregisterValueChangedCallback(OnMusicUpdated);
            }
        }
        
        private void OnDeviceVolumeReceived(float mediaValue)
        {
            if (m_CachedMediaVolume != mediaValue)
                m_CachedMediaVolume = mediaValue;
            
            // Global Volume
            var logVolume = Mathf.Log(mediaValue + k_MinVolume) * 20f + m_MaxVolume;
            SetMasterVolume(logVolume); 
            
            // Vivox Volume
            SetSpeakerVolume(mediaValue * 100f);
        }

        private void OnMasterVolumeUpdated(ChangeEvent<int> value)
        {
            SetMasterVolume(Mathf.Log10((value.newValue + k_MinVolume) / 100f) * 20f + m_MaxVolume);
        }

        private void SetMasterVolume(float volume)
        {
            AudioMaster.Instance.volume.audioMixer.SetFloat("volume", volume);
        }

        private void OnSoundFXUpdated(ChangeEvent<int> value)
        {
            AudioMaster.Instance.soundFX.audioMixer.SetFloat("sound-fx",
                Mathf.Log10((value.newValue + k_MinVolume) / 100f) * 20 + m_MaxSoundFX);
        }

        private void OnMusicUpdated(ChangeEvent<int> value)
        {
            AudioMaster.Instance.music.audioMixer.SetFloat("music",
                Mathf.Log10((value.newValue + k_MinVolume) / 100f) * 20 + m_MaxMusic);
        }

        private void OnVivoxMicrophoneVolumeUpdated(ChangeEvent<int> value)
        {
            SetMicrophoneVolume(value.newValue);
        }

        private void SetMicrophoneVolume(float volume)
        {
            if(VivoxManager.Instance != null)
                VivoxManager.Instance.Devices.SetMicrophoneVolume((int) (volume + k_MinVolume));
        }

        private void OnVivoxVolumeUpdated(ChangeEvent<int> value)
        {
            SetSpeakerVolume(value.newValue);
        }

        private void SetSpeakerVolume(float volume)
        {
            if(VivoxManager.Instance != null)
                VivoxManager.Instance.Devices.SetSpeakerVolume((int)(volume + k_MinVolume));
        }

        public override void Reset()
        {
            base.Reset();
            ResetSliderIntCurrentState(m_VolumeSlider);
            ResetSliderIntCurrentState(m_SoundFXSlider);
        }
    }
}