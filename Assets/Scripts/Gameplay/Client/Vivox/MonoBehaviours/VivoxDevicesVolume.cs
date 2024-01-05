using Unity.Services.Vivox;
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
                if (VivoxService.Instance != null && VivoxService.Instance.Client != null)
                    return VivoxService.Instance.Client.AudioInputDevices.Muted;
                return true;
            }
        }

        public void SetMicrophoneVolume(int volume)
        {
            if (VivoxService.Instance == null || VivoxService.Instance.Client == null)
                return;

            VivoxService.Instance.Client.AudioInputDevices.VolumeAdjustment = volume - k_VivoxVolumeRange;
        }

        public void SetSpeakerVolume(int volume)
        {
            if (VivoxService.Instance == null || VivoxService.Instance.Client == null)
                return;

            VivoxService.Instance.Client.AudioOutputDevices.VolumeAdjustment = volume - k_VivoxVolumeRange;
        }

        public void SetMicrophoneMute(bool value)
        {
            if (VivoxService.Instance == null || VivoxService.Instance.Client == null)
                return;

            VivoxService.Instance.Client.AudioInputDevices.Muted = value;
        }

        public void SetMuteSpeaker(bool value)
        {
            if (VivoxService.Instance == null || VivoxService.Instance.Client == null)
                return;

            VivoxService.Instance.Client.AudioOutputDevices.Muted = value;
        }
    }
}