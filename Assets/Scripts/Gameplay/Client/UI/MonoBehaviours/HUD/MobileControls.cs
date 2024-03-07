#if !UNITY_STANDALONE
using Unity.MegacityMetro.CameraManagement;
using Unity.MegacityMetro.Gameplay;
#endif
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.MegacityMetro.UI
{
    public class MobileControls : MonoBehaviour
    {
        [SerializeField] private GameObject m_VirtualJoystickPrefab;
        public VirtualJoystick JoystickLeft { get; private set; }
        public static MobileControls Instance { get; private set; }
        public float Speed => m_SpeedSlider.value;
        public bool Shoot { get; private set; }
        
        private Slider m_SpeedSlider;
        private Button m_ShootButton;
        private VisualElement m_JoystickZone;
        private VisualElement m_FireZone;
        private VisualElement m_SpeedDragger;
        private VisualElement m_SliderUpArrow;
        private VisualElement m_SliderDownArrow;
        private VisualElement m_SliderDragContainer;
        private Label m_SpeedLabel;

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

        private void Start()
        {
            var uiDocument = GetComponent<UIDocument>();
            var root = uiDocument.rootVisualElement;
#if !UNITY_STANDALONE
            m_SpeedSlider = root.Q<Slider>("speed-slider");
            m_ShootButton = root.Q<Button>("shoot-button");
            m_JoystickZone = root.Q<VisualElement>("joystick-zone");
            m_FireZone = root.Q<VisualElement>("fire-zone");
            m_SpeedDragger = m_SpeedSlider.Q<VisualElement>("unity-dragger");
            m_SliderUpArrow = root.Q<VisualElement>("up-arrow");
            m_SliderDownArrow = root.Q<VisualElement>("down-arrow");
            m_SliderDragContainer = root.Q<VisualElement>("unity-drag-container");

            if (PlayerInfoController.Instance.IsSinglePlayer)
            {
                m_FireZone.style.display = DisplayStyle.None;
            }

            JoystickLeft = CreateJoystick();

            // Add speed label to dragger
            m_SpeedLabel = new Label("0");
            m_SpeedLabel.AddToClassList("speed-slider-label");
            m_SpeedDragger.Add(m_SpeedLabel);
            
            // Callbacks
            m_FireZone.RegisterCallback<PointerDownEvent>(OnFireZonePressed, TrickleDown.TrickleDown);
            m_FireZone.RegisterCallback<PointerUpEvent>(OnFireZoneReleased);
            m_FireZone.RegisterCallback<PointerLeaveEvent>(OnFireZoneLeave);
            m_JoystickZone.RegisterCallback<PointerDownEvent>(OnJoystickZonePressed);
            m_SliderDragContainer.RegisterCallback<PointerUpEvent>(OnSpeedSliderReleased);
            m_SpeedSlider.RegisterValueChangedCallback(OnSpeedSliderChanged);
#else
            root.style.display = DisplayStyle.None;
#endif
        }

#if !UNITY_STANDALONE
        private void OnSpeedSliderChanged(ChangeEvent<float> evt)
        {
            var newValue = evt.newValue;
            m_SpeedLabel.text = (newValue * 100).ToString();

            if (newValue >= 0.75f)
            {
                m_SliderUpArrow.style.opacity = 0.1f;
            }
            else if (newValue <= -0.75f)
            {
                m_SliderDownArrow.style.opacity = 0.1f;
            }
            else
            {
                m_SliderUpArrow.style.opacity = 1f;
                m_SliderDownArrow.style.opacity = 1f;
            }
        }

        private void OnFireZonePressed(PointerDownEvent evt)
        {
            Shoot = true;
            m_ShootButton.style.display = DisplayStyle.Flex;
            m_ShootButton.transform.position = evt.position;
        }
        
        private void OnFireZoneReleased(PointerUpEvent evt)
        {
            Shoot = false;
            m_ShootButton.style.display = DisplayStyle.None;
        }
        
        private void OnFireZoneLeave(PointerLeaveEvent evt)
        {
            Shoot = false;
            m_ShootButton.style.display = DisplayStyle.None;
        }

        private void OnJoystickZonePressed(PointerDownEvent pointerDownEvent)
        {
            JoystickLeft.SetPosition(pointerDownEvent.position, pointerDownEvent);
        }

        private void OnSpeedSliderReleased(PointerUpEvent e)
        {
            if (m_SpeedSlider.value is < 0.2f and > -0.2f)
            {
                m_SpeedSlider.SetValueWithoutNotify(0);
                m_SpeedLabel.text = "0";
            }
        }
        
        private VirtualJoystick CreateJoystick()
        {
            var prefab = Instantiate(m_VirtualJoystickPrefab, transform.parent, true);
            var joystick = prefab.GetComponent<VirtualJoystick>();
            return joystick;
        }
#endif
    }
}