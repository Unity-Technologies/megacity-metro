using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Unity.MegacityMetro.CameraManagement
{
    /// <summary>
    /// Post Processing Bloom Modifier
    /// </summary>
    public class PostProcessingBloomModifier : MonoBehaviour
    {
        enum State
        {
            None,
            PingPongAnimation,
            ExitingAnimation
        }

        [SerializeField]
        private Color m_Color;
        [SerializeField]
        private Color m_ColorSecondary;
        [SerializeField]
        private float m_Duration;

        private Bloom m_CurrentBloom;
        private Color m_CachedColor;
        private bool m_OverrideState;
        private VolumeParameter<Color> m_Parameter;

        private float m_Time;
        private State m_State;

        private void Awake()
        {
            var assetProfile = GetComponent<Volume>();
            var profile = assetProfile.sharedProfile;
            if (!profile.TryGet(out m_CurrentBloom))
                m_CurrentBloom = profile.Add<Bloom>(false);

            m_Parameter = new VolumeParameter<Color>();
            m_OverrideState = m_CurrentBloom.tint.overrideState;
            m_CachedColor = m_CurrentBloom.tint.value;
            m_CurrentBloom.tint.overrideState = true;
            m_State = State.None;
        }

        private void OnDisable()
        {
            if (m_CurrentBloom != null)
            {
                m_CurrentBloom.tint.overrideState = m_OverrideState;
                m_CurrentBloom.tint.value = m_CachedColor;
            }
        }

        public void GenerateColorTransition()
        {
            m_State = State.PingPongAnimation;
        }

        public void StopColorTransition()
        {
            m_Time = m_Duration;
            m_State = State.ExitingAnimation;
        }

        private void Update()
        {
            if (m_State == State.None) return;

            if (m_State == State.PingPongAnimation)
            {
                m_Parameter.value = Color.Lerp(m_Color, m_ColorSecondary, Mathf.PingPong(Time.time, m_Duration));
                m_CurrentBloom.tint.SetValue(m_Parameter);
            }
            else if (m_State == State.ExitingAnimation)
            {
                m_Time -= Time.deltaTime;
                m_Parameter.value = Color.Lerp(m_CachedColor, m_Color, Mathf.PingPong(m_Time, m_Duration));
                m_CurrentBloom.tint.SetValue(m_Parameter);
            }

            if (m_Time <= 0 && m_State == State.ExitingAnimation)
            {
                m_State = State.None;
                m_Parameter.value = m_CachedColor;
                m_CurrentBloom.tint.SetValue(m_Parameter);
            }
        }
    }

}
