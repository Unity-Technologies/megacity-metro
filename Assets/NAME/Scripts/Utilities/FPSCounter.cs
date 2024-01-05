using System;
using System.Collections;
using UnityEngine;
using System.Text;

namespace Unity.NAME.Utilities
{
    // Provides a quick and dirty performance counter

    public class FpsCounter : MonoBehaviour
    {
        public TextMesh FpsCounterText;
        public TextMesh SnapshotText;
        public int UpdateEveryNFrames = 1;
        public static Action<bool> RunTest;

        readonly StringBuilder m_StrAccum = new StringBuilder(32);
        readonly float[] m_Samples = new float[120];
        int m_NumSamples = 0;
        int m_NextSampleIndex = 0; //into ring buffer

        float m_Min, m_Max, m_StdDev;
        private int m_FrameCounter = 0;

        static float s_TestSamples = 0;
        static int s_TestFrames = 0;
        static bool s_RunTest = false;


        void Awake()
        {
            gameObject.SetActive(false);
        }

        void OnRunTest(bool set)
        {
            if (set == s_RunTest) return;
            Debug.Log($"FPSCounter OnRunTest {set}\n");
            StartCoroutine(_RunTest(set, 1.5f));
        }

        IEnumerator _RunTest(bool set, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            s_RunTest = set;

            if (set)
            {
                SnapshotText.text = "Taking snapshot...";
                s_TestFrames = 0;
                s_TestSamples = 0;
            }
            else
            {
                if (s_TestFrames != 0)
                {
                    float ave = s_TestFrames / s_TestSamples;
                    SnapshotText.text = $"TEST OVER {s_TestSamples}s\nDT: {1000f / ave}\nFPS: {ave}";
                }
            }
        }

        void Start()
        {
            SnapshotText.text = "";
            RunTest += OnRunTest;
            if (FpsCounterText == null)
            {
                Debug.LogError("FPSCounter: no textbox, aborting.");
                enabled = false;
                return;
            }
        }

        void Update()
        {
            if (s_RunTest)
            {
                s_TestSamples += Time.unscaledDeltaTime;
                s_TestFrames++;
            }

            float newSample = Time.unscaledDeltaTime;
            m_Samples[m_NextSampleIndex] = newSample;
            m_NumSamples = Mathf.Min(m_Samples.Length, m_NumSamples + 1);
            m_NextSampleIndex = (m_NextSampleIndex + 1) % m_Samples.Length;

            m_FrameCounter = (m_FrameCounter + 1) % UpdateEveryNFrames;
            if (m_FrameCounter == 0)
            {
                float sum = 0.0f;
                //  max = samples[0];
                //  min = samples[0];
                for (int i = 0; i < m_NumSamples; i++)
                {
                    sum += m_Samples[i];
                    //  max = Mathf.Max(samples[i], max);
                    //      min = Mathf.Min(samples[i], min);
                }
                float ave = sum / (float)m_NumSamples;
                /*
                float stdDevSum = 0.0f;
                if (numSamples > 0)
                {
                    for(int i=0; i<numSamples; i++)
                    {
                        float diffMean = (samples[i] - ave);
                        stdDevSum += diffMean*diffMean;
                    }

                    //note: using /N instead of /(N-1) for stddev
                    stdDev = stdDevSum / (float)(numSamples-1);
                    stdDev = Mathf.Sqrt(stdDev);
                }
                else
                {
                    stdDev = -1.0f; //not valid
                }
                */
                float aveFps = 1.0f / ave;
                //float minFPS = 1.0f / min;
                //float maxFPS = 1.0f / max;
                m_StrAccum.Clear();
                m_StrAccum.Append("AVE:\n");
                m_StrAccum.Append("\tDT: ").Append((ave * 1000.0f).ToString("n2")).Append("ms\n");
                m_StrAccum.Append("\tFPS: ").Append(aveFps.ToString("n2")).Append("FPS\n");
                //strAccum.Append("StdDev: ").Append(stdDev.ToString("n3")).Append("ms\n");

                /*
                strAccum.Append("FASTEST: \n");
                strAccum.Append("\tDT: ").Append((min*1000.0f).ToString("n2")).Append("ms\n");
                strAccum.Append("\tFPS: ").Append((minFPS).ToString("n2")).Append("FPS\n");
                strAccum.Append("SLOWEST: \n");
                strAccum.Append("\tDT: ").Append((max*1000.0f).ToString("n2")).Append("ms\n");
                strAccum.Append("\tFPS: ").Append((maxFPS).ToString("n2")).Append("FPS\n");
                */
                FpsCounterText.text = m_StrAccum.ToString();
                m_StrAccum.Clear();
            }
        }
    }
}
