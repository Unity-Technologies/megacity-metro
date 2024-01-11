using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Unity.MegacityMetro.CameraManagement
{
    /// <summary>
    /// Handles the Dead Screen FX
    /// </summary>
    public class DeadScreenFX : MonoBehaviour
    {
        [SerializeField]
        private Texture2D m_TextureLut;
        [SerializeField, Range(0,1)]
        private float m_LutIntensity;
        [SerializeField, Range(-100, 100)]
        private float m_Saturation;

        private ColorAdjustments m_ColorAdjustments;
        private ChromaticAberration m_ChromaticAberration;

        private float m_CachedSaturation;
        private Texture2DParameter m_CachedTextureLut;
        private Texture2DParameter m_TextureLutParameter;
        private float m_CachedLutIntensity;
        private float m_Time;
        private bool m_EnableTransition;

        public bool IsRunning => m_EnableTransition;

        private void Awake()
        {
            var assetProfile = GetComponent<Volume>();
            var profile = assetProfile.sharedProfile;
            if (!profile.TryGet(out m_ColorAdjustments))
                m_ColorAdjustments = profile.Add<ColorAdjustments>(false);

            if (!profile.TryGet(out m_ChromaticAberration))
                m_ChromaticAberration = profile.Add<ChromaticAberration>(false);

            m_EnableTransition = false;
            m_CachedSaturation = m_ColorAdjustments.saturation.value;
            m_CachedLutIntensity = m_ChromaticAberration.intensity.value;
        }

        private void OnDisable()
        {
            StopGeneratingEffect();
        }

        public void GeneratingEffectTransition()
        {
            m_EnableTransition = true;
            m_Time = 0;
        }

        public void StopGeneratingEffect()
        {
            m_EnableTransition = false;
            if (m_ColorAdjustments != null)
            {
                m_ColorAdjustments.saturation.value = m_CachedSaturation;
            }

            if (m_ChromaticAberration != null)
            {
                m_ChromaticAberration.intensity.value = m_CachedLutIntensity;
            }
        }

        private void Update()
        {
            if (m_EnableTransition)
            {
                m_ChromaticAberration.intensity.value = math.lerp(m_CachedLutIntensity, m_LutIntensity, m_Time);
                m_ColorAdjustments.saturation.value = math.lerp(m_CachedSaturation, m_Saturation, m_Time);
                m_Time += Time.deltaTime;
            }
        }
    }
}

