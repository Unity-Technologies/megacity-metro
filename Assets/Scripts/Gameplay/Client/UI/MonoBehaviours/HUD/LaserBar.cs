using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.MegacityMetro.UI
{
    /// <summary>
    /// LaserBar UI element
    /// </summary>
    public class LaserBar : MonoBehaviour
    {
        private ProgressBar m_LaserBar;
        private Label m_PowerValue;

        private void Awake()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            m_LaserBar = root.Q<ProgressBar>("laser-bar");
            m_PowerValue = root.Q<Label>("power-value");
        }

        public void UpdateBar(float energy)
        {
            m_LaserBar.value = energy;
            m_PowerValue.text = $"{(int)energy}";
        }
    }
}
