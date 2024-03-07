using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.MegacityMetro.UI
{
    /// <summary>
    /// Crosshair UI element
    /// </summary>
    public class Crosshair : MonoBehaviour
    {
        private VisualElement m_Crosshair;
        private VisualElement m_AimIcon;
        private VisualElement Panel;
        private Camera m_Camera;
        private bool m_AimIconVisible;

        public bool IsVisible => m_Crosshair.style.display == DisplayStyle.Flex;

        private void Awake()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            m_Crosshair = root.Q<VisualElement>("crosshair");
            m_AimIcon = root.Q<VisualElement>("aim-icon");
            Panel = m_Crosshair.parent;
            m_Camera = Camera.main;
        }

        public void Show()
        {
            m_Crosshair.style.display = DisplayStyle.Flex;
        }

        public void Hide()
        {
            m_Crosshair.style.display = DisplayStyle.None;
        }

        public void SetTarget(bool value)
        {
            if (value)
            {
                m_AimIcon.style.display = DisplayStyle.Flex;
                m_AimIcon.AddToClassList("crosshair-aim-highlight");
            }
            else
            {
                if (m_AimIcon.ClassListContains("crosshair-aim-highlight"))
                    m_AimIcon.RemoveFromClassList("crosshair-aim-highlight");
                m_AimIcon.style.display = DisplayStyle.None;
            }
        }

        public void UpdatePosition(float3 placerPosition)
        {
            var screenPosition = m_Camera.WorldToScreenPoint(placerPosition);
            var panelPosition = RuntimePanelUtils.ScreenToPanel(Panel.panel,
                new Vector2(screenPosition.x, Screen.height - screenPosition.y));
            panelPosition.x -= m_Crosshair.contentRect.size.x / 2;
            panelPosition.y -= m_Crosshair.contentRect.size.y / 2;
            m_AimIcon.transform.position = new Vector3(panelPosition.x, panelPosition.y, 0);
            m_Crosshair.transform.position = math.lerp(m_Crosshair.transform.position, new Vector3(panelPosition.x, panelPosition.y, 0), 0.2f);
        }
    }
}