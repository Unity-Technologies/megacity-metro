using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Unity.MegacityMetro.CameraManagement
{
    /// <summary>
    /// Vignette Post Processing Modifier
    /// </summary>
    public class PostProcessingVignetteModifier : MonoBehaviour
    {
        [SerializeField] private Volume m_Volume;
        [SerializeField] private float m_TargetIntensity = 0.6f;
        [SerializeField] private float m_Duration = 1f;

        private Vignette m_Vignette;
        private bool m_OverrideState;
        private float m_CachedIntensity;

        private float m_Time;
        private bool m_ActiveEffect;

        private void Awake()
        {
            var profile = m_Volume.sharedProfile;
            if (!profile.TryGet(out m_Vignette))
                m_Vignette = profile.Add<Vignette>();

            m_OverrideState = m_Vignette.intensity.overrideState;
            m_CachedIntensity = m_Vignette.intensity.value;
            m_Vignette.intensity.overrideState = true;
        }

        private void OnDisable()
        {
            if (m_Vignette != null)
            {
                m_Vignette.intensity.overrideState = m_OverrideState;
                m_Vignette.intensity.value = m_CachedIntensity;
            }
        }

        public void IncreaseVignette(bool value)
        {
            StartCoroutine(SetVignetteIntensity(value));
        }


        private IEnumerator SetVignetteIntensity(bool value)
        {
            var elapsedTime = 0f;
            var targetIntensity = value ? m_TargetIntensity : m_CachedIntensity;
            var currentIntensity = m_Vignette.intensity.value;
            while (elapsedTime < m_Duration)
            {
                m_Vignette.intensity.value = math.lerp(currentIntensity, targetIntensity, elapsedTime / m_Duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
    }
}