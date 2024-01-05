using Unity.NAME.Game;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.NAME.UI
{
    // This is the component that is responsible for the Objectives display in the top left corner of the screen

    [RequireComponent(typeof(RectTransform))]
    public class ObjectiveToast : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Text content that will display the title")]
        public TMPro.TextMeshProUGUI TitleTextContent;

        [Tooltip("Text content that will display the description")]
        [SerializeField]
        protected TMPro.TextMeshProUGUI DescriptionTextContent;
        [Tooltip("Text content that will display the counter")]
        public TMPro.TextMeshProUGUI CounterTextContent;

        [Tooltip("Rect that will display the description")]
        public RectTransform SubTitleRect;
        [Tooltip("Canvas used to fade in and out the content")]
        public CanvasGroup CanvasGroup;

        [Header("Transitions")]
        [Tooltip("Delay before moving complete")]
        public float CompletionDelay;
        [Tooltip("Duration of the fade in")]
        public float FadeInDuration = 0.5f;
        [Tooltip("Duration of the fade out")]
        public float FadeOutDuration = 2f;

        [Header("Sound")]
        [Tooltip("Sound that will be player on initialization")]
        public AudioClip InitSound;
        [Tooltip("Sound that will be player on completion")]
        public AudioClip CompletedSound;

        [Header("Movement")]
        [Tooltip("Time it takes to move in the screen")]
        public float MoveInDuration = 0.5f;
        [Tooltip("Animation curve for move in, position in x over time")]
        public AnimationCurve MoveInCurve;

        [Tooltip("Time it takes to move out of the screen")]
        public float MoveOutDuration = 2f;
        [Tooltip("Animation curve for move out, position in x over time")]
        public AnimationCurve MoveOutCurve;

        float m_StartFadeTime;
        bool m_IsFadingIn;
        bool m_IsFadingOut;
        bool m_IsMovingIn;
        bool m_IsMovingOut;
        AudioSource m_AudioSource;
        RectTransform m_RectTransform;

        public void Initialize(string titleText, string descText, string counterText, bool isOptionnal, float delay)
        {
            // set the description for the objective, and forces the content size fitter to be recalculated
            Canvas.ForceUpdateCanvases();

            m_RectTransform = GetComponent<RectTransform>();

            TitleTextContent.text = titleText;
            SetDescriptionText(descText);
            CounterTextContent.text = counterText;

            LayoutRebuilder.ForceRebuildLayoutImmediate(m_RectTransform);

            m_StartFadeTime = Time.time + delay;
            // start the fade in
            m_IsFadingIn = true;
            m_IsMovingIn = true;
        }

        public void Complete()
        {
            m_StartFadeTime = Time.time + CompletionDelay;
            m_IsFadingIn = false;
            m_IsMovingIn = false;

            // if a sound was set, play it
            PlaySound(CompletedSound);

            // start the fade out
            m_IsFadingOut = true;
            m_IsMovingOut = true;
        }

        public void SetDescriptionText(string text)
        {
            DescriptionTextContent.text = text;
            SubTitleRect.gameObject.SetActive(!string.IsNullOrEmpty(text));
        }

        void Update()
        {
            float timeSinceFadeStarted = Time.time - m_StartFadeTime;

            if (m_IsFadingIn && !m_IsFadingOut)
            {
                // fade in
                if (timeSinceFadeStarted < FadeInDuration)
                {
                    // calculate alpha ratio
                    CanvasGroup.alpha = timeSinceFadeStarted / FadeInDuration;
                }
                else
                {
                    CanvasGroup.alpha = 1f;
                    // end the fade in
                    m_IsFadingIn = false;

                    PlaySound(InitSound);
                }
            }

            if (m_IsMovingIn && !m_IsMovingOut)
            {
                // move in
                if (timeSinceFadeStarted < MoveInDuration)
                {
                    m_RectTransform.anchoredPosition =
                        new Vector2((int)MoveInCurve.Evaluate(timeSinceFadeStarted / MoveInDuration), m_RectTransform.anchoredPosition.y);
                }
                else
                {
                    // making sure the position is exact
                    m_RectTransform.anchoredPosition = new Vector2(0, m_RectTransform.anchoredPosition.y);

                    m_IsMovingIn = false;
                }
            }

            if (m_IsFadingOut)
            {
                // fade out
                if (timeSinceFadeStarted < FadeOutDuration)
                {
                    // calculate alpha ratio
                    CanvasGroup.alpha = 1 - (timeSinceFadeStarted) / FadeOutDuration;
                }
                else
                {
                    CanvasGroup.alpha = 0f;

                    // end the fade out, then destroy the object
                    m_IsFadingOut = false;
                    gameObject.SetActive(false);
                }
            }

            if (m_IsMovingOut)
            {
                // move out
                if (timeSinceFadeStarted < MoveOutDuration)
                {
                    m_RectTransform.anchoredPosition =
                        new Vector2((int)MoveOutCurve.Evaluate(timeSinceFadeStarted / MoveOutDuration),
                            m_RectTransform.anchoredPosition.y);
                }
                else
                {
                    m_IsMovingOut = false;
                }
            }
        }

        void PlaySound(AudioClip sound)
        {
            if (!sound)
                return;

            if (!m_AudioSource)
            {
                m_AudioSource = gameObject.AddComponent<AudioSource>();
                m_AudioSource.outputAudioMixerGroup = AudioUtility.GetAudioGroup(AudioUtility.AudioGroups.HUDObjective);
            }

            m_AudioSource.PlayOneShot(sound);
        }
    }
}
