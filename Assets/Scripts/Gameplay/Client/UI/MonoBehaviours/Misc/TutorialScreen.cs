using System.Collections;
using Unity.MegacityMetro.Gameplay;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UIElements;

namespace Unity.MegacityMetro.UI
{
    /// <summary>
    /// Tutorial Screen UI element
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class TutorialScreen : MonoBehaviour
    {
        public static TutorialScreen Instance { get; private set; }

        private VisualElement m_TutorialScreen;
        private VisualElement m_SinglePlayerTutorial;
        private VisualElement m_MultiplayerTutorial;
        private VisualElement m_MobileTutorial;
        private VisualElement m_MobileMultiplayerTutorial;
        private VisualElement m_ShootButton;
        private VisualElement m_LeaderboardInstructions;
        private VisualElement m_JoystickHandler;
        private VisualElement m_SpeedSlider;
        private bool m_InTutorialScreen;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            m_TutorialScreen = root.Q<VisualElement>("tutorial-screen");
            m_SinglePlayerTutorial = root.Q<VisualElement>("tutorial-single-player");
            m_MultiplayerTutorial = root.Q<VisualElement>("tutorial-multiplayer");
            m_MobileTutorial = root.Q<VisualElement>("tutorial-mobile");
            m_ShootButton = root.Q<VisualElement>("shoot-button");
            m_LeaderboardInstructions = root.Q<VisualElement>("leaderboard-instructions");
            m_JoystickHandler = root.Q<VisualElement>("handle");
            m_SpeedSlider = root.Q<VisualElement>("speed-slider");
        }

        private void Start()
        {
            if (PlayerInfoController.Instance == null)
                return;

            ShowTutorial();

#if UNITY_ANDROID || UNITY_IPHONE
            m_MobileTutorial.style.display = DisplayStyle.Flex;
            m_SinglePlayerTutorial.style.display = DisplayStyle.None;
            m_MultiplayerTutorial.style.display = DisplayStyle.None;

            if (PlayerInfoController.Instance.IsSinglePlayer)
            {
                m_ShootButton.style.display = DisplayStyle.None;
                m_LeaderboardInstructions.style.display = DisplayStyle.None;
            }
            else
            {
                m_ShootButton.style.display = DisplayStyle.Flex;
                m_LeaderboardInstructions.style.display = DisplayStyle.Flex;
            }
#else
            m_MobileTutorial.style.display = DisplayStyle.None;
            
            if (PlayerInfoController.Instance.IsSinglePlayer)
            {
                m_SinglePlayerTutorial.style.display = DisplayStyle.Flex;
                m_MultiplayerTutorial.style.display = DisplayStyle.None;
            }
            else
            {
                m_MultiplayerTutorial.style.display = DisplayStyle.Flex;
                m_SinglePlayerTutorial.style.display = DisplayStyle.None;
            }
#endif
        }

        public void ShowTutorial()
        {
            if (m_InTutorialScreen)
                return;

            m_TutorialScreen.style.display = DisplayStyle.Flex;
            m_InTutorialScreen = true;

            StartCoroutine(WaitForAnyInput());
#if UNITY_ANDROID || UNITY_IPHONE
            StartCoroutine(AnimateMobileTutorial());
#endif
        }

        private void HideTutorial()
        {
            m_TutorialScreen.style.display = DisplayStyle.None;
            m_InTutorialScreen = false;
        }

        private IEnumerator WaitForAnyInput()
        {
            while (m_InTutorialScreen)
            {
                InputSystem.onAnyButtonPress.CallOnce(_ => { HideTutorial(); });
                yield return null;
            }
        }
        
        private IEnumerator AnimateMobileTutorial()
        {
            while (m_InTutorialScreen)
            {
                yield return new WaitForSeconds(1f);
                m_JoystickHandler.transform.position = Random.insideUnitCircle * 50f;
                m_SpeedSlider.transform.position = new Vector3(0, Random.Range(-100, 100), 0);
                yield return new WaitForSeconds(1f);
                m_JoystickHandler.transform.position = Vector3.zero;
            }
        }
    }
}