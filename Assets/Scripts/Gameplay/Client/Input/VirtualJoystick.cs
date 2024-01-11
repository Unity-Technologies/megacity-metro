using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.MegacityMetro.UI
{
    public class VirtualJoystick : MonoBehaviour
    {
        private VisualElement m_JoystickBackground;
        private VisualElement m_Handle;
        private VisualElement m_Joystick;

        private Vector2 m_Position;
        private bool m_PressArea;

        [SerializeField] private float AreaMultiplier = 1f;
        [SerializeField] private Vector2 DefaultPosition = new(-200, -200);
        public Vector2 Delta { get; private set; }
        private PanelSettings m_PanelSettings;


#if (UNITY_ANDROID || UNITY_IPHONE || ENABLED_VIRTUAL_JOYSTICK)
        private void OnEnable()
        {
            var uiDocument = GetComponent<UIDocument>();
            var root = uiDocument.rootVisualElement;
            m_PanelSettings = uiDocument.panelSettings;
            m_JoystickBackground = root.Q<VisualElement>("background");
            m_Handle = root.Q<VisualElement>("handle");
            m_Joystick = root.Q<VisualElement>("joystick");

            m_Handle.RegisterCallback<PointerDownEvent>(OnPressed);
            m_Handle.RegisterCallback<PointerUpEvent>(OnReleased);
            m_Handle.RegisterCallback<PointerMoveEvent>(OnMoved);

            m_JoystickBackground.RegisterCallback<PointerDownEvent>(OnPressedJoystickArea);
            m_JoystickBackground.RegisterCallback<PointerUpEvent>(OnReleaseJoystickArea);
            m_JoystickBackground.RegisterCallback<PointerMoveEvent>(OnMovePointer);

            m_Joystick.style.display = DisplayStyle.Flex;
            Reset();
        }

#else
        private void Start()
        {
            Destroy(gameObject);
        }
#endif

#if UNITY_EDITOR
        private void Update()
        {
            if (!UnityEngine.Cursor.visible)
                CursorUtils.ShowCursor();
        }
#endif

        private void OnPressedJoystickArea(PointerDownEvent e)
        {
            m_PressArea = true;
            m_JoystickBackground.CapturePointer(e.pointerId);
            TranslateHandle(e.position, m_JoystickBackground.worldBound.position,
                (m_JoystickBackground.worldBound.size / 2));
        }

        private void OnPressed(PointerDownEvent e)
        {
            m_Handle.CapturePointer(e.pointerId);
            m_Position = e.position;
            m_Joystick.style.opacity = 1;
        }

        private void OnReleased(PointerUpEvent e)
        {
            m_Handle.ReleasePointer(e.pointerId);
            m_Handle.transform.position = Vector3.zero;
            Delta = Vector2.zero;
            m_PressArea = false;
            Reset();
        }

        private void OnReleaseJoystickArea(PointerUpEvent e)
        {
            m_JoystickBackground.ReleasePointer(e.pointerId);
            if (m_PressArea)
            {
                m_Handle.transform.position = Vector3.zero;
                Delta = Vector2.zero;
            }

            m_PressArea = false;
            m_Joystick.style.opacity = 0;
        }

        private void OnMoved(PointerMoveEvent e)
        {
            if (!m_Handle.HasPointerCapture(e.pointerId))
                return;

            TranslateHandle(e.position, Vector2.zero, m_Position);
        }

        private void OnMovePointer(PointerMoveEvent e)
        {
            if (m_PressArea)
                TranslateHandle(e.position, m_JoystickBackground.worldBound.position,
                    (m_JoystickBackground.worldBound.size / 2));
        }

        private void TranslateHandle(Vector2 newPosition, Vector2 positionRelativeToParent, Vector2 currentPosition)
        {
            var maxDelta = ((m_JoystickBackground.worldBound.size - m_Handle.worldBound.size) / 2) * AreaMultiplier;
            var mappedPosition = ClampPosition(newPosition - positionRelativeToParent - currentPosition, -maxDelta,
                maxDelta);
            m_Handle.transform.position = mappedPosition / AreaMultiplier;
            Delta = mappedPosition / maxDelta;
        }

        private Vector2 ClampPosition(Vector2 vector, Vector2 min, Vector2 max)
        {
            var x = math.clamp(vector.x, min.x, max.x);
            var y = math.clamp(vector.y, min.y, max.y);
            return new Vector2(x, y);
        }

        public void Reset()
        {
            m_Joystick.style.left = DefaultPosition.x;
            m_Joystick.style.bottom = DefaultPosition.y;
            m_Joystick.style.opacity = 0;
        }

        public void SetPosition(Vector2 position, PointerDownEvent evt)
        {
            m_Joystick.style.left = position.x - m_JoystickBackground.worldBound.width / 2;
            m_Joystick.style.bottom = (m_PanelSettings.referenceResolution.y - position.y) - m_JoystickBackground.worldBound.height / 2 ;
            OnPressed(evt);
        }
    }
}