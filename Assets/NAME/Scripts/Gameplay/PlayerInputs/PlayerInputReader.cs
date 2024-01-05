using Unity.NAME.Game;
using UnityEngine;

namespace Unity.NAME.Gameplay
{
    // A component that handles the reading of Inputs for the main character

    public class PlayerInputReader : MonoBehaviour
    {
        public Vector2 MovementDirection { get; private set; }
        public bool Jump { get; private set; }

        GameInputActions m_InputActions;

        void Awake()
        {
            m_InputActions = new GameInputActions();
            m_InputActions.Player.Move.performed += ctx => MovementDirection = ctx.ReadValue<Vector2>();
            m_InputActions.Player.Jump.started += ctx => Jump = true;

            EventManager.AddListener<GameStateChangeEvent>(OnGameStateChangeEvent);
        }

        void OnGameStateChangeEvent(GameStateChangeEvent evt)
        {
            MovementDirection = Vector2.zero;
            Jump = false;

            switch (evt.NewGameState)
            {
                case GameState.Play: m_InputActions?.Enable(); break;
                default: m_InputActions?.Disable(); break;
            }
        }

        void LateUpdate()
        {
            Jump = false;
        }

        void OnDestroy()
        {
            EventManager.RemoveListener<GameStateChangeEvent>(OnGameStateChangeEvent);
        }
    }
}
