using System.Collections;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace Unity.MegacityMetro.CameraManagement
{
    /// <summary>
    /// Handles the Post Processing Glitch
    /// </summary>
    public class PostProcessingGlitch : MonoBehaviour
    {
        [FormerlySerializedAs("volume")] [SerializeField]
        private Volume m_Volume;


        [SerializeField] private float block = 0.2f;
        [SerializeField] private float drift = 0.1f;
        [SerializeField] private float jitter = 0.3f;
        [SerializeField] private float jump = 0.05f;
        [SerializeField] private float shake = 0.05f;

        private IEnumerator m_EnableGlitchCoroutine;

        private void Awake()
        {
            var profile = m_Volume.sharedProfile;
        }

        private void OnDisable()
        {
            SetGlitchEnabled(false);
        }

        public void SetGlitchEnabled(bool value)
        {
            if (value)
            {
                m_EnableGlitchCoroutine = EnableGlitch();
                StartCoroutine(m_EnableGlitchCoroutine);
            }
            else
            {
                if (m_EnableGlitchCoroutine != null)
                    StopCoroutine(m_EnableGlitchCoroutine);
            }
        }

        private IEnumerator EnableGlitch()
        {
            var elapsedTime = 0f;
            var targetBlock = block;
            var targetDrift = drift;
            var targetJitter = jitter;
            var targetJump = jump;
            var targetShake = shake;
            while (elapsedTime < 1f)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
    }
}