using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace Unity.MegacityMetro.UI
{
    /// <summary>
    /// Notification UI element
    /// </summary>
    public class Notification : MonoBehaviour
    {
        [SerializeField]
        private float m_TimeOnScreen = 1.5f;
        private Label m_Notification;
        private VisualElement m_TopMessageContainer;

        private void Awake()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            m_TopMessageContainer = root.Q<VisualElement>("top-message-container");
            m_Notification = root.Q<Label>("top-message-label");
        }

        private void Show()
        {
            m_TopMessageContainer.style.display = DisplayStyle.Flex;
        }

        private void Hide()
        {
            m_TopMessageContainer.style.display = DisplayStyle.None;
        }

        public void Message(string message)
        {
            m_Notification.text = message;
            m_TopMessageContainer.style.top = 50;
            m_TopMessageContainer.style.opacity = 0;
            Show();
            m_TopMessageContainer.experimental.animation.Start(new StyleValues{top = 150, opacity = 1}, 500).Ease(Easing.OutBack).OnCompleted(() =>
            {
                StartCoroutine(HideMessage());
            });
        }

        private IEnumerator HideMessage()
        {
            yield return new WaitForSeconds(m_TimeOnScreen);
            m_TopMessageContainer.experimental.animation.Start(new StyleValues{top = 50, opacity = 0}, 500).Ease(Easing.OutBack).OnCompleted(Hide);
        }
    }
}
