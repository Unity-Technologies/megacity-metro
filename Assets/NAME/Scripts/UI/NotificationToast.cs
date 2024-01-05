using UnityEngine;

namespace Unity.NAME.UI
{
    // That script describes the notifications that appear in the bottom left corner of the screen
    // when the status of an objective changes

    public class NotificationToast : MonoBehaviour
    {
        [Tooltip("Text content that will display the notification text")]
        public TMPro.TextMeshProUGUI TextContent;
        [Tooltip("Canvas used to fade in and out the content")]
        public CanvasGroup CanvasGroup;
        [Tooltip("How long it will stay visible")]
        public float VisibleDuration;
        [Tooltip("Duration of the fade in")]
        public float FadeInDuration = 0.5f;
        [Tooltip("Duration of the fade out")]
        public float FadeOutDuration = 2f;

        public bool Initialized { get; private set; }
        float m_InitTime;

        public float TotalRunTime => VisibleDuration + FadeInDuration + FadeOutDuration;

        public void Initialize(string text)
        {
            TextContent.text = text;
            m_InitTime = Time.time;

            // start the fade out
            Initialized = true;
        }

        void Update()
        {
            if (Initialized)
            {
                float timeSinceInit = Time.time - m_InitTime;
                if (timeSinceInit < FadeInDuration)
                {
                    // fade in
                    CanvasGroup.alpha = timeSinceInit / FadeInDuration;
                }
                else if (timeSinceInit < FadeInDuration + VisibleDuration)
                {
                    // stay visible
                    CanvasGroup.alpha = 1f;
                }
                else if (timeSinceInit < FadeInDuration + VisibleDuration + FadeOutDuration)
                {
                    // fade out
                    CanvasGroup.alpha = 1 - (timeSinceInit - FadeInDuration - VisibleDuration) / FadeOutDuration;
                }
                else
                {
                    CanvasGroup.alpha = 0f;

                    // fade out over, destroy the object
                    Initialized = false;
                    //Destroy(gameObject);
                }
            }
        }
    }
}
