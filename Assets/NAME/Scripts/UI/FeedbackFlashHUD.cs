using Unity.NAME.Game;
using UnityEngine;

namespace Unity.NAME.UI
{
    // This is the manager that handles the red flash when the time is almost done for a specific objective (if it's time based)

    public class FeedbackFlashHUD : MonoBehaviour
    {
        [Header("References")]

        [Tooltip("CanvasGroup to fade the critical time vignette")]
        public CanvasGroup VignetteCanvasGroup;

        [Header("Critical time")]
        [Tooltip("Max alpha of the critical vignette")]
        public float CriticalTimeVignetteMaxAlpha = .8f;
        [Tooltip("Frequency at which the vignette will pulse when at critical health")]
        public float PulsatingVignetteFrequency = 4f;
        [Tooltip("Show the critical time vignette when the remaining time reaches this point")]
        public float ShowCriticalTimeVignetteWhen = 5f;
        [Tooltip("Audio clip for the critical time")]
        public AudioClip WarningAudioClip;
        [Tooltip("Audio Source in the Scene")]
        public AudioSource m_AudioSource;
        bool m_FlashActive;

        bool m_WarningSoundPlayed = false;
        TimeManager m_TimeManager;
        GameState m_CurrentGameState;

        void OnGameStateChange(GameStateChangeEvent evt) => m_CurrentGameState = evt.NewGameState;

        void Awake()
        {
            EventManager.AddListener<GameStateChangeEvent>(OnGameStateChange);
        }

        void Start()
        {
            m_TimeManager = ServiceLocator.GetService<TimeManager>();
            //m_AudioSource = GetComponent<AudioSource>();
            //DebugUtils.HandleErrorIfNullFindObject<AudioSource, FeedbackFlashHUD>(m_AudioSource, this);
        }

        void Update()
        {
            if (!m_TimeManager.TimeSet)
                return;

            if (m_TimeManager.TimeRemaining < ShowCriticalTimeVignetteWhen)
            {
                EnableFlash(true);
                float vignetteAlpha = CriticalTimeVignetteMaxAlpha;

                switch (m_CurrentGameState)
                {
                    case GameState.Success:
                        VignetteCanvasGroup.alpha = vignetteAlpha;
                        break;
                    case GameState.Failure:
                        VignetteCanvasGroup.alpha = 0;
                        break;
                    default:
                        VignetteCanvasGroup.alpha = ((Mathf.Cos(m_TimeManager.TimeRemaining * PulsatingVignetteFrequency * 2f * Mathf.PI) + 1.0f) * 0.5f) * vignetteAlpha;

                        if (!m_WarningSoundPlayed && VignetteCanvasGroup.alpha >= 0.5f)
                        {
                            m_AudioSource.PlayOneShot(WarningAudioClip);
                            m_WarningSoundPlayed = true;
                        }

                        if (VignetteCanvasGroup.alpha < 0.5f)
                        {
                            m_WarningSoundPlayed = false;
                        }
                        break;
                }
            }
            else if (m_TimeManager.TimeRemaining > ShowCriticalTimeVignetteWhen)
            {
                EnableFlash(false);
            }
        }

        void EnableFlash(bool set)
        {
            if (m_FlashActive == set)
                return;

            VignetteCanvasGroup.gameObject.SetActive(set);
            m_FlashActive = set;

            if (!set)
                VignetteCanvasGroup.alpha = 0;
        }
    }
}
