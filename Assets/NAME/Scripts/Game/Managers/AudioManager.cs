using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace Unity.NAME.Game
{
    // An Audio manager that handles the destruction of instantiated SFX

    public class AudioManager : MonoBehaviour
    {
        public AudioMixer AudioMixer;

        public void EnsureSFXDestruction(AudioSource source)
        {
            StartCoroutine("DelayedSFXDestruction", source);
        }

        private IEnumerator DelayedSFXDestruction(AudioSource source)
        {
            while (source.isPlaying)
            {
                yield return null;
            }

            GameObject.Destroy(source.gameObject);
        }
    }
}
