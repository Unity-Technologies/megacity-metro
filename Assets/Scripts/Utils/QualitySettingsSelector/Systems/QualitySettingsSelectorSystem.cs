#if AUTO_QUALITY_SETTINGS

using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Burst;

namespace Unity.QualitySettingsTools
{
    [BurstCompile]
    public partial struct QualitySettingsSystem : ISystem
    {
        private int m_MinTargetFrameRate;
        private int m_CheckInterval;
        private int m_FrameCount;

        public void OnCreate(ref SystemState state)
        {
            state.Enabled = !Application.isEditor;
            m_MinTargetFrameRate = 60;
            m_CheckInterval = 60; // Check once per second (assuming targetFrameRate = 60)
            m_FrameCount = 0;
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Check the current FPS once per second (assuming targetFrameRate = 60)
            if (m_FrameCount % m_CheckInterval == 0)
            {
                int currentFps = (int)math.round(1.0f / state.WorldUnmanaged.Time.DeltaTime);
                if (currentFps < m_MinTargetFrameRate)
                {
                    DecreaseQualityLevel();
                }
            }

            m_FrameCount++;
        }

        private void DecreaseQualityLevel()
        {
            int currentQualityLevel = QualitySettings.GetQualityLevel();
            int newQualityLevel = math.max(0, currentQualityLevel - 1);
            QualitySettings.SetQualityLevel(newQualityLevel, true); // Apply expensive changes
        }
    }
}

#endif