using UnityEngine;

namespace Unity.QualitySettingsTools
{
    /// <summary>
    /// Activates children of this object based on the current quality settings.
    /// </summary>
    public class QualitySettingsObjectActivator : MonoBehaviour
    {
        private enum Quality
        {
            Optimal = 0,
            Medium = 1,
            HD = 2,
        }

        [SerializeField]
        private Quality m_MinimalTargetQuality;
        [SerializeField]
        private int m_CheckIntervalInFrames = 360;
        private int m_FrameCount;

        private Transform[] children;
        private void Start()
        {
            m_FrameCount = 0;
            children = transform.GetComponentsInChildren<Transform>();
        }

        private void Update()
        {
            if (m_FrameCount % m_CheckIntervalInFrames == 0)
            {
                CheckCurrentQualitySettings();
            }

            m_FrameCount++;
        }

        private void CheckCurrentQualitySettings()
        {
            int currentQualityLevel = QualitySettings.GetQualityLevel();
            if (currentQualityLevel < (int)m_MinimalTargetQuality)
            {
                foreach (var child in children)
                {
                    child.gameObject.SetActive(false);
                }
                gameObject.SetActive(false);
            }
        }
    }
}
