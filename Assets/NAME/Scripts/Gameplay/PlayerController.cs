using Unity.NAME.Game;
using UnityEngine;

namespace Unity.NAME.Gameplay
{
    // The component that makes the link between the PlayerInputReader and the BodyController
    // It gets the values from the Inputs and forwards them to the body controller

    [RequireComponent(typeof(BodyController), typeof(PlayerInputReader))]
    public class PlayerController : MonoBehaviour
    {
        BodyController m_BodyController;
        PlayerInputReader m_InputReader;
        ThirdPersonCamera m_Camera;

        void Awake()
        {
            m_BodyController = GetComponent<BodyController>();
            m_InputReader = GetComponent<PlayerInputReader>();
        }

        void Update()
        {
            if (m_Camera == null)
            {
                m_Camera = ServiceLocator.GetService<ICameraService>()?.GetCamera<ThirdPersonCamera>();
            }

            if (m_Camera != null)
            {
                m_BodyController.SetMovementDirection((m_Camera.HorizontalVectorReference * m_InputReader.MovementDirection.x)
                    + (m_Camera.VerticalVectorReference * m_InputReader.MovementDirection.y));
            }

            if (m_BodyController.CanJump() && m_InputReader.Jump)
            {
                m_BodyController.StartJump();
            }
        }
    }
}
