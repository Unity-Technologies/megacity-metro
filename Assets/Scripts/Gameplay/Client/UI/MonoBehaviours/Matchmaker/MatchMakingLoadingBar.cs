using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace Unity.MegacityMetro.UI
{
    /// <summary>
    /// Matchmaking loading bar UI element
    /// </summary>
    public class MatchMakingLoadingBar
    {
        private VisualElement m_MatchMakingLodingBar;
        private VisualElement m_MatchMakingLodingBar0;
        private VisualElement m_MatchMakingLodingBar1;

        public MatchMakingLoadingBar(VisualElement container)
        {
            m_MatchMakingLodingBar = container.Q<VisualElement>("matchmaking-loading-bar");
            m_MatchMakingLodingBar0 = container.Q<VisualElement>("matchmaking-loading-bar-0");
            m_MatchMakingLodingBar1 = container.Q<VisualElement>("matchmaking-loading-bar-1");
        }

        public void Enable(bool enable)
        {
            m_MatchMakingLodingBar.style.display = enable ? DisplayStyle.Flex : DisplayStyle.None;
            if (enable)
                AnimateLoadingBar();
        }

        private void AnimateLoadingBar()
        {
            if (m_MatchMakingLodingBar.style.display == DisplayStyle.Flex)
            {
                var currentRotation = m_MatchMakingLodingBar0.style.rotate;
                var angle = currentRotation.value.angle.value;
                var quaternionA = Quaternion.Euler(0f, 0f, angle + 180f);
                var quaternionB = Quaternion.Inverse(quaternionA);
                m_MatchMakingLodingBar0.experimental.animation.Rotation(quaternionA, 500).easingCurve = Easing.Linear;
                m_MatchMakingLodingBar1.experimental.animation.Rotation(quaternionB, 500).OnCompleted(AnimateLoadingBar).easingCurve = Easing.Linear;
            }
        }
    }
}