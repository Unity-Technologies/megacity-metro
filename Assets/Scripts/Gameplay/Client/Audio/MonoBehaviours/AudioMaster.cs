using System;
using UnityEngine;
using UnityEngine.Audio;

namespace Unity.MegacityMetro.Audio
{
    /// <summary>
    /// Saves the constant parameters for Audio Traffic Pool,
    /// This Includes access to the AudioMixer groups.
    /// </summary>
    public class AudioMaster : MonoBehaviour
    {
#if UNITY_ANDROID
        public event Action<float> OnDeviceVolumeReceived;
#endif
        public bool showDebugLines;
        public float maxDistance = 90f;
        public int maxVehicles = 16;
        public int closestEmitterPerClipCount = 3;
        public AudioMixerGroup soundFX;
        public AudioMixerGroup volume;
        public AudioMixerGroup music;

        public AudioSource menuMusic;
        
        public static AudioMaster Instance { private set; get; }

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
            
            if(Application.isBatchMode)
                menuMusic.Stop();
        }

#if UNITY_ANDROID
        public void OnReceiveDeviceVolume(string mediaVolumeParam)
        {
            if (OnDeviceVolumeReceived != null && 
                float.TryParse(mediaVolumeParam, out var mediaVolume))
            {
                OnDeviceVolumeReceived(mediaVolume);
            }
        }
#endif
    }
}
