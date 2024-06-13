using UnityEngine;

namespace Unity.MegacityMetro.Gameplay
{
    /// <summary>
    /// Manages Vivox devices volume
    /// </summary>
    public class VivoxDevicesVolume : MonoBehaviour
    {
        // vivox min value is -50 while max value is 50
        private const int k_VivoxVolumeRange = 50;
        
        public bool Muted
        {
            get
            {
                if (VivoxManager.Instance != null && VivoxManager.Instance.Service != null)
                    return VivoxManager.Instance.Service.IsInputDeviceMuted;
                return true;
            }
        }

        public void SetMicrophoneVolume(int volume)
        {
            if (VivoxManager.Instance == null || VivoxManager.Instance.Service == null)
                return;
            
            VivoxManager.Instance.Service.SetInputDeviceVolume(volume - k_VivoxVolumeRange);
        }

        public void SetSpeakerVolume(int volume)
        {
            if (VivoxManager.Instance == null || VivoxManager.Instance.Service == null)
                return;
            
            VivoxManager.Instance.Service.SetOutputDeviceVolume(volume - k_VivoxVolumeRange);
        }

        public void SetMicrophoneMute(bool value)
        {
            if (VivoxManager.Instance == null || VivoxManager.Instance.Service == null)
                return;
            
            if (value)
            {
                VivoxManager.Instance.Service.MuteInputDevice();
            }
            else
            {
                VivoxManager.Instance.Service.UnmuteInputDevice();
            }
        }

        public void SetMuteSpeaker(bool value)
        {
            if (VivoxManager.Instance == null || VivoxManager.Instance.Service == null)
                return;

            if (value)
            {
                VivoxManager.Instance.Service.MuteOutputDevice();
            }
            else
            {
                VivoxManager.Instance.Service.UnmuteOutputDevice();
            }
        }
    }
}