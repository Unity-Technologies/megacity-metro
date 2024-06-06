using UnityEngine;
using UnityEngine.InputSystem;

namespace Unity.MegacityMetro.UI
{
    public class InputReader : MonoBehaviour
    {
        private GameInput m_GameInput;

        private void OnEnable()
        {
            if (m_GameInput == null)
            {
                m_GameInput = new GameInput();
            }

            EnableUIInput();

            m_GameInput.UI.Navigate.performed += OnNavigate;
            m_GameInput.UI.Submit.performed += OnSubmit;
            m_GameInput.UI.Cancel.performed += OnCancel;
            m_GameInput.UI.Back.performed += OnCancel;
        }

        private void EnableUIInput()
        {
            m_GameInput.UI.Enable();
        }

        private void OnDisable()
        {
            m_GameInput.UI.Disable();
        }

        public void OnNavigate(InputAction.CallbackContext context)
        {
            UIEvents.OnNavigate?.Invoke(context.ReadValue<Vector2>());
        }

        public void OnSubmit(InputAction.CallbackContext context)
        {
            UIEvents.OnSubmit?.Invoke();
        }

        public void OnCancel(InputAction.CallbackContext context)
        {
            UIEvents.OnCancel?.Invoke();
        }
    }
}