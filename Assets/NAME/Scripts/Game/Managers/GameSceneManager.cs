using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.NAME.Game
{
    public class GameSceneManager : MonoBehaviour
    {
        [Tooltip("The Scene to load when the player goes back to the Introduction Menu")]
        public string IntroScene;
        [Tooltip("The Scene to load when the player wins")]
        public string SuccessScene;
        [Tooltip("The Scene to load when the player loses")]
        public string FailureScene;
        [Tooltip("All the levels of the game")]
        public List<string> MainLevels;

        private static GameSceneManager s_Instance;

        void Awake()
        {
            if (s_Instance == null)
            {
                s_Instance = this;
                DontDestroyOnLoad(gameObject);

                EventManager.AddListener<GameStateChangeEvent>(OnGameStateChange);
                EventManager.AddListener<UserMenuActionEvent>(OnUserMenuAction);
            }
            else if (s_Instance != this)
            {
                Destroy(gameObject);
            }
        }

        void OnGameStateChange(GameStateChangeEvent evt)
        {
            if (evt.NewGameState == GameState.Menu)
            {
                switch (evt.CurrentGameState)
                {
                    case GameState.Success: SceneManager.LoadScene(SuccessScene); break;
                    case GameState.Failure: SceneManager.LoadScene(FailureScene); break;
                    default: SceneManager.LoadScene(IntroScene); break;
                }
            }
        }

        void OnUserMenuAction(UserMenuActionEvent evt)
        {
            switch (evt.UserMenuAction)
            {
                case UserMenuAction.Play: SceneManager.LoadScene(MainLevels[0]); break;
                case UserMenuAction.ReturnToIntroMenu: SceneManager.LoadScene(IntroScene); break;
            }
        }

        void OnDestroy()
        {
            if (s_Instance == this)
            {
                EventManager.RemoveListener<GameStateChangeEvent>(OnGameStateChange);
                EventManager.RemoveListener<UserMenuActionEvent>(OnUserMenuAction);
            }
        }
    }
}
