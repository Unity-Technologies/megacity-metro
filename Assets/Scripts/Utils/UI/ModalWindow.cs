using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace Unity.MegacityMetro.UI
{
    /// <summary>
    /// Modal Window UI element
    /// </summary>
    public class ModalWindow : MonoBehaviour
    {
        public static ModalWindow Instance { get; private set; }

        public Action OnHideModal;
        [SerializeField] private InputAction BackAction;
        private VisualElement m_Container;
        private VisualElement m_ModalWindowPanel;
        private Label m_ModalMessage;
        private Button m_CancelButton;
        private Button m_ConfirmButton;

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
            m_Container = root.Q<VisualElement>("modal-window-container");
            m_ModalWindowPanel = root.Q<VisualElement>("modal-window-panel");
            m_ModalMessage = root.Q<Label>("modal-main-message");
            m_CancelButton = root.Q<Button>("modal-cancel-button");
            m_ConfirmButton = root.Q<Button>("modal-confirm-button");

            BackAction.performed += _ => OnBackAction();
        }

        private void OnBackAction()
        {
            Hide();
        }

        private void AnimateIn()
        {
            m_Container.style.opacity = 0;
            m_Container.style.display = DisplayStyle.Flex;
            m_Container.experimental.animation.Start(new StyleValues { opacity = 1 }, 500);
            m_ModalWindowPanel.experimental.animation
                .Start(new StyleValues { top = -100f }, new StyleValues { top = 0 }, 500).Ease(Easing.OutCubic)
                .OnCompleted(() =>
                {
                    BackAction.Enable();
                    m_ConfirmButton.Focus();
                });
        }

        private void AnimateOut()
        {
            m_Container.experimental.animation.Start(new StyleValues { opacity = 0 }, 500);
            m_ModalWindowPanel.experimental.animation
                .Start(new StyleValues { top = 0 }, new StyleValues { top = -100f }, 500)
                .Ease(Easing.OutCubic)
                .OnCompleted(() =>
                {
                    m_Container.style.display = DisplayStyle.None;
                    BackAction.Disable();
                });
        }

        public void Show(string message, string confirmText, string cancelText, Action onConfirm,
            Action onCancel = null)
        {
            m_ModalMessage.text = message;
            m_ConfirmButton.text = confirmText;
            m_CancelButton.text = cancelText;
            m_CancelButton.style.display = DisplayStyle.Flex;
            m_ConfirmButton.clickable.clicked += () =>
            {
                onConfirm?.Invoke();
                Hide();
            };
            m_CancelButton.clickable.clicked += () =>
            {
                onCancel?.Invoke();
                Hide();
            };

            AnimateIn();
        }

        public void Show(string message, string confirmText, Action onConfirm = null)
        {
            CursorUtils.ShowCursor();

            m_ModalMessage.text = message;
            m_ConfirmButton.text = confirmText;
            m_CancelButton.style.display = DisplayStyle.None;
            m_ConfirmButton.clickable.clicked += () =>
            {
                onConfirm?.Invoke();
                Hide();
            };

            AnimateIn();
        }

        private void Hide()
        {
            OnHideModal?.Invoke();
            AnimateOut();
        }
    }
}