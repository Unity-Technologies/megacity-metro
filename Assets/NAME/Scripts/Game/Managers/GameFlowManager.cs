using System.Collections;
using UnityEngine;

namespace Unity.NAME.Game
{
    // The Root component for the game. It sets the GameState and handles the End scene loading (Win or lose)
    // It also broadcasts events to notify the different systems of a GameState change

    public class GameFlowManager : MonoBehaviour
    {
        [Header("Parameters")]
        [Tooltip("Duration of the fade-to-black at the end of the game")]
        public float EndSceneLoadDelay = 3f;
        [Tooltip("The canvas group of the fade-to-black screen")]
        public CanvasGroup EndGameFadeCanvasGroup;

        [Header("Win")]
        [Tooltip("Win game message")]
        public string WinDisplayMessage = "You WIN!";
        [Tooltip("Duration of delay before the win message")]
        public float DelayBeforeWinMessage = 1f;
        [Tooltip("Sound played on win")]
        public AudioClip VictorySound;
        [Tooltip("Duration of delay before the fade-to-black, if winning")]
        public float DelayBeforeFadeToBlack = 2f;

        [Header("Lose")]
        [Tooltip("Lose game message")]
        public string LoseDisplayMessage = "You Lose...";

        public GameState GameState { get; private set; }

        ObjectiveManager m_ObjectiveManager;
        TimeManager m_TimeManager;
        float m_TimeLoadEndGameScene;
        float m_ElapsedTimeBeforeEndScene = 0;

        void Start()
        {
            m_ObjectiveManager = ServiceLocator.GetService<ObjectiveManager>();
            m_TimeManager = ServiceLocator.GetService<TimeManager>();

            AudioUtility.SetMasterVolume(1);

            StartCoroutine(ShowObjectivesRoutine());
            StartCoroutine(WaitThenStartGameRoutine());
        }

        IEnumerator WaitThenStartGameRoutine()
        {
            yield return new WaitForEndOfFrame();

            if (m_TimeManager.TimeSet)
            {
                m_TimeManager.StartTimer();
                while (!m_TimeManager.TimerHasStarted)
                    yield return null;
            }

            SetGameState(GameState.Play);
        }

        IEnumerator ShowObjectivesRoutine()
        {
            yield return new WaitForSecondsRealtime(0.2f);

            for (int i = 0; i < m_ObjectiveManager.Objectives.Count; i++)
            {
                if (m_ObjectiveManager.Objectives[i].ObjectiveMessage != "")
                {
                    DisplayMessageEvent evt = Events.DisplayMessageEvent;
                    evt.MessageText = m_ObjectiveManager.Objectives[i].ObjectiveMessage;
                    evt.DelayBeforeDisplay = 0.0f;
                    EventManager.Broadcast(evt);
                }
                yield return new WaitForSecondsRealtime(1f);
            }
        }

        void Update()
        {
            if (GameState != GameState.Play)
            {
                m_ElapsedTimeBeforeEndScene += Time.deltaTime;
                if (m_ElapsedTimeBeforeEndScene >= EndSceneLoadDelay)
                {
                    float timeRatio = Mathf.Clamp01(1 - (m_TimeLoadEndGameScene - Time.time));
                    EndGameFadeCanvasGroup.alpha = timeRatio;

                    float volumeRatio = Mathf.Abs(timeRatio);
                    float volume = Mathf.Clamp(1 - volumeRatio, 0, 1);
                    AudioUtility.SetMasterVolume(volume);

                    // See if it's time to switch the game state (after the delay)
                    if (Time.time >= m_TimeLoadEndGameScene)
                    {
                        SetGameState(GameState.Menu);
                    }
                }
            }
            else
            {
                if (m_ObjectiveManager.AreAllObjectivesCompleted())
                    EndGame(true);

                if (m_TimeManager.IsOver)
                    EndGame(false);
            }
        }

        void EndGame(bool win)
        {
            EndGameFadeCanvasGroup.gameObject.SetActive(true);
            m_TimeManager.StopTimer();

            // unlocks the cursor before leaving the scene, to be able to click buttons
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            m_TimeLoadEndGameScene = Time.time + EndSceneLoadDelay + DelayBeforeFadeToBlack;

            // Remember that we need to load the appropriate end scene after a delay
            if (win)
            {
                SetGameState(GameState.Success);

                // play a sound on win
                var audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.clip = VictorySound;
                audioSource.playOnAwake = false;
                audioSource.outputAudioMixerGroup = AudioUtility.GetAudioGroup(AudioUtility.AudioGroups.HUDVictory);
                audioSource.PlayScheduled(AudioSettings.dspTime + DelayBeforeWinMessage);
            }
            else
            {
                SetGameState(GameState.Failure);
            }

            DisplayMessageEvent evt = Events.DisplayMessageEvent;
            evt.MessageText = win ? WinDisplayMessage : LoseDisplayMessage;
            evt.DelayBeforeDisplay = DelayBeforeWinMessage;
            EventManager.Broadcast(evt);
        }

        void SetGameState(GameState newGameState)
        {
            // Broadcast game state change
            GameStateChangeEvent evt = Events.GameStateChangeEvent;
            evt.CurrentGameState = GameState;
            evt.NewGameState = newGameState;
            EventManager.Broadcast(evt);

            GameState = newGameState;
        }
    }
}
