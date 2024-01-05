using UnityEngine;
using UnityEngine.Audio;

namespace Unity.NAME.Game
{
    // This is the Audio Utility file.
    // It contains static functions to provide a series of options to handle the sound InGame
    // - Creating an SFX
    // - Gathering the Audio Group
    // - Change the Master Volume
    // - And get the Master Volume

    public class AudioUtility
    {
        static AudioManager s_AudioManager;

        public enum AudioGroups
        {
            Pickup,
            HUDVictory,
            HUDObjective
        }

        public static void CreateSFX(AudioClip clip, Vector3 position, AudioGroups audioGroup, float spatialBlend, float rolloffDistanceMin = 1f)
        {
            GameObject impactSFXInstance = new GameObject("SFX_" + clip.name);
            impactSFXInstance.transform.position = position;
            AudioSource source = impactSFXInstance.AddComponent<AudioSource>();
            source.clip = clip;
            source.spatialBlend = spatialBlend;
            source.minDistance = rolloffDistanceMin;
            source.Play();

            source.outputAudioMixerGroup = GetAudioGroup(audioGroup);

            s_AudioManager.EnsureSFXDestruction(source);
        }

        public static AudioMixerGroup GetAudioGroup(AudioGroups group)
        {
            if (s_AudioManager == null)
                s_AudioManager = Object.FindObjectOfType<AudioManager>();

            var groups = s_AudioManager.AudioMixer.FindMatchingGroups(group.ToString());

            if (groups.Length > 0)
                return groups[0];

            Debug.LogWarning("Didn't find audio group for " + group.ToString());
            return null;
        }

        public static void SetMasterVolume(float value)
        {
            if (s_AudioManager == null)
                s_AudioManager = Object.FindObjectOfType<AudioManager>();

            if (value <= 0)
                value = 0.001f;
            float valueInDb = Mathf.Log10(value) * 20;

            s_AudioManager.AudioMixer.SetFloat("MasterVolume", valueInDb);
        }

        public static float GetMasterVolume()
        {
            if (s_AudioManager == null)
                s_AudioManager = Object.FindObjectOfType<AudioManager>();

            s_AudioManager.AudioMixer.GetFloat("MasterVolume", out var valueInDb);
            return Mathf.Pow(10f, valueInDb / 20.0f);
        }
    }
}
