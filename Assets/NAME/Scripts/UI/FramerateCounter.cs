using UnityEngine;
using TMPro;

namespace Unity.NAME.UI
{
    // A simple component to display the FPS at runtime, in the top right corner of the screen
    // It is accessible through the InGameMenu

    public class FramerateCounter : MonoBehaviour
    {
        [Tooltip("Delay between updates of the displayed framerate value")]
        public float PollingTime = 0.5f;
        [Tooltip("The text field displaying the framerate")]
        public TextMeshProUGUI UiText;

        float m_AccumulatedDeltaTime = 0f;
        int m_AccumulatedFrameCount = 0;

        void Update()
        {
            m_AccumulatedDeltaTime += Time.deltaTime;
            m_AccumulatedFrameCount++;

            if (m_AccumulatedDeltaTime >= PollingTime)
            {
                int framerate = Mathf.RoundToInt((float)m_AccumulatedFrameCount / m_AccumulatedDeltaTime);
                UiText.text = framerate.ToString();

                m_AccumulatedDeltaTime = 0f;
                m_AccumulatedFrameCount = 0;
            }
        }
    }
}
