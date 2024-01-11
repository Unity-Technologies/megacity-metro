using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.MegacityMetro.Utils
{
    /// <summary>
    /// keeps the data for Line Doppler and applies according to the delay parameters.
    /// </summary>
    public class DelayLineDopplerHack : MonoBehaviour
    {
        public AudioListener m_Listener;

        public GameObject m_Proto;

        public int m_NumSources = 30;

        public float m_MaxDistance = 1.0f;
        public float m_Haas = 2000.0f;
        public float m_EnvelopeSpeed = 2.0f;
        public float m_PanAmount = 2.0f;
        public float m_Radius = 500.0f;
        public float m_SwitchTimeMin = 0.01f;
        public float m_SwitchTimeMax = 4.0f;
        public float m_DopplerLevel = 1.0f;

        struct SourceData
        {
            public GameObject m_Object;
            public float[] m_Attenuation;
            public float m_Distance;
            public float m_InterpolatedDistance;
            public float[] m_InterpolatedAttenuation;
            public Vector3 m_Follow;
            public Vector3 m_Target;
            public float m_TimeUntilNextUpdate;
            public float m_MoveTime;
        }

        SourceData[] m_SourceData;
        float[] m_Delay = new float[0x40000];
        int m_WritePos;
        int m_SampleRate;
        bool m_Ready;

        // Start is called before the first frame update
        void Start()
        {
            m_SampleRate = 44100;
            m_SourceData = new SourceData[m_NumSources];
        }

        System.Random r = new System.Random();

        // Update is called once per frame
        void Update()
        {
            if (!m_Ready)
            {
                for (int i = 0; i < m_SourceData.Length; i++)
                {
                    var s = m_SourceData[i];
                    s.m_Object = Object.Instantiate(m_Proto);
                    s.m_Attenuation = new float[2];
                    s.m_InterpolatedAttenuation = new float[2];
                    m_SourceData[i] = s;
                }

                m_Proto.SetActive(false);
            }

            for (int i = 0; i < m_SourceData.Length; i++)
            {
                var s = m_SourceData[i];
                s.m_TimeUntilNextUpdate -= Time.deltaTime;

                if (s.m_TimeUntilNextUpdate <= 0.0f)
                {
                    s.m_MoveTime = m_SwitchTimeMin + (m_SwitchTimeMax - m_SwitchTimeMin) * (float)r.NextDouble();
                    s.m_TimeUntilNextUpdate += s.m_MoveTime;

                    var x = ((float)r.NextDouble() * 2.0f - 1.0f) * m_Radius;
                    var y = ((float)r.NextDouble() * 2.0f - 1.0f) * m_Radius;
                    var z = ((float)r.NextDouble() * 2.0f - 1.0f) * m_Radius;
                    s.m_Target = new Vector3(x, y, z);
                    if (!m_Ready)
                    {
                        s.m_Follow = s.m_Target;
                        s.m_Object.transform.position = s.m_Target;
                        x = ((float)r.NextDouble() * 2.0f - 1.0f) * m_Radius;
                        y = ((float)r.NextDouble() * 2.0f - 1.0f) * m_Radius;
                        z = ((float)r.NextDouble() * 2.0f - 1.0f) * m_Radius;
                        s.m_Target = new Vector3(x, y, z);
                    }
                }

                m_SourceData[i] = s;
            }

            float distanceScale = 1.0f / m_MaxDistance;

            for (int i = 0; i < m_SourceData.Length; i++)
            {
                var target = m_SourceData[r.Next(m_SourceData.Length - 1)].m_Object.transform.position;
                var s = m_SourceData[i];
                float moveSpeed = 1.0f - Mathf.Pow(0.1f, Time.deltaTime / s.m_MoveTime);
                s.m_Follow += (s.m_Target - s.m_Follow) * moveSpeed;
                var delta = (s.m_Follow - s.m_Object.transform.position) * moveSpeed;
                s.m_Object.transform.position += delta;
                s.m_Object.transform.rotation = Quaternion.LookRotation(-delta.normalized);
                var dir = m_Listener.transform.worldToLocalMatrix.MultiplyVector(s.m_Object.transform.position);
                var len = dir.magnitude;
                var leftDir = 0.5f * dir.z - dir.x;
                var rightDir = 0.5f * dir.z + dir.x;
                var attenuation = 1.0f / (1.0f + s.m_Distance * distanceScale);
                s.m_Attenuation[0] = attenuation * (0.65f + m_PanAmount * 0.45f *
                    Mathf.Clamp(leftDir * Mathf.Abs(leftDir) / (len + 0.001f), -1.0f, 1.0f));
                s.m_Attenuation[1] = attenuation * (0.65f + m_PanAmount * 0.45f *
                    Mathf.Clamp(rightDir * Mathf.Abs(rightDir) / (len + 0.001f), -1.0f, 1.0f));
                s.m_Distance = (m_Listener.transform.position - s.m_Object.transform.position).magnitude;
                m_SourceData[i] = s;
            }

            if (!m_Ready)
            {
                for (int i = 0; i < m_SourceData.Length; i++)
                {
                    var s = m_SourceData[i];
                    s.m_InterpolatedDistance = s.m_Distance;
                    m_SourceData[i] = s;
                }

                m_Ready = true;
            }
        }

        void OnAudioFilterRead(float[] data, int numChannels)
        {
            if (!m_Ready)
            {
                for (int n = 0; n < data.Length; n++)
                    data[n] = 0.0f;
                return;
            }

            float envelopeSpeed = 1.0f - Mathf.Pow(0.001f, 1.0f / (m_EnvelopeSpeed * m_SampleRate));

            int maxLength = (int)(m_Delay.Length / numChannels) - 1;

            float dopplerSamples = m_DopplerLevel * m_SampleRate / 340.0f;

            for (int n = 0; n < data.Length; n += numChannels)
            {
                for (int c = 0; c < numChannels; c++)
                {
                    m_Delay[m_WritePos] = data[n + c];
                    if (++m_WritePos == m_Delay.Length)
                        m_WritePos = 0;
                    data[n + c] = 0.0f;
                }

                for (int i = 0; i < m_SourceData.Length; i++)
                {
                    var s = m_SourceData[i];

                    s.m_InterpolatedDistance += (s.m_Distance - s.m_InterpolatedDistance) * envelopeSpeed;

                    int delaySamplesBase = (int)(s.m_InterpolatedDistance * dopplerSamples);

                    for (int c = 0; c < numChannels; c++)
                    {
                        var haasDelay = (int)((s.m_Attenuation[0] - s.m_Attenuation[1]) * m_Haas * (c * 2 - 1));
                        var delaySamples = Mathf.Clamp(delaySamplesBase + haasDelay, 0, maxLength);

                        int readPos = m_WritePos - numChannels - delaySamples * numChannels;
                        if (readPos < 0)
                            readPos += m_Delay.Length;

                        s.m_InterpolatedAttenuation[c] +=
                            (s.m_Attenuation[c] - s.m_InterpolatedAttenuation[c]) * envelopeSpeed;
                        data[n + c] += m_Delay[readPos] * s.m_InterpolatedAttenuation[c];
                        if (++readPos >= m_Delay.Length)
                            readPos = 0;
                    }

                    m_SourceData[i] = s;
                }
            }
        }
    }
}
