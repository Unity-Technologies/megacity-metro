using System.Collections;
using Unity.Mathematics;
using Unity.MegacityMetro.Gameplay;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace Unity.MegacityMetro.UI
{
    /// <summary>
    /// Manages the HUD UI elements.
    /// </summary>
    [RequireComponent(typeof(Crosshair))]
    [RequireComponent(typeof(LaserBar))]
    [RequireComponent(typeof(Notification))]
    [RequireComponent(typeof(UIDocument))]
    public class HUD : MonoBehaviour
    {
        [SerializeField] private UILeaderboard m_Leaderboard;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        [SerializeField] private NetcodePanelStats m_NetcodePanelPrefab;
#endif

        private ProgressBar m_HealthBar;
        private Crosshair m_Crosshair;
        private Notification m_Notification;
        private LaserBar m_LaserBar;

        private VisualElement m_AttackerPointerContainer;
        private VisualElement m_AttackerPointer;
        private VisualElement m_MessageScreen;
        private VisualElement m_DamageIndicator;
        private Button m_SettingsButton;

        private Label m_BottomMessageLabel;
        private Label m_MessageLabel;
        private Label m_ControllerLabel;
        private Label m_HealthBarLabel;

        private float m_DeathCooldown = 5f;
        private float m_DamageIndicatorMaxOpacity = 0.6f;
        private float m_DamageIndicatorSpeed = 2;

        public static HUD Instance { get; private set; }
        public UILeaderboard Leaderboard => m_Leaderboard;
        public Crosshair Crosshair => m_Crosshair;
        public LaserBar Laser => m_LaserBar;
        public Notification Notification => m_Notification;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            m_Crosshair = GetComponent<Crosshair>();
            m_Notification = GetComponent<Notification>();
            m_LaserBar = GetComponent<LaserBar>();
        }

        private void Start()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            m_HealthBar = root.Q<ProgressBar>("health-bar");
            m_HealthBarLabel = root.Q<Label>("health-bar-value");
            m_DamageIndicator = root.Q<VisualElement>("damage-indicator");
            m_MessageScreen = root.Q<VisualElement>("message-screen");
            m_MessageLabel = m_MessageScreen.Q<Label>("message-label");
            m_BottomMessageLabel = m_MessageScreen.Q<Label>("bottom-message-label");
            m_SettingsButton = root.Q<Button>("hud-settings-button");
            m_AttackerPointerContainer = root.Q<VisualElement>("attacker-pointer-container");
            m_AttackerPointer = root.Q<VisualElement>("attacker-pointer");

            m_AttackerPointerContainer.style.display = DisplayStyle.None;
            m_MessageScreen.style.display = DisplayStyle.None;

            CursorUtils.HideCursor();

            m_SettingsButton.clicked += () => { GameSettingsOptionsMenu.Instance.ShowSettingsOptions(true); };

            if (PlayerInfoController.Instance.IsSinglePlayer)
            {
                var hideInSinglePlayerList = root.Query(className: "hide-in-single-player").ToList();
                foreach (var singlePlayerElement in hideInSinglePlayerList)
                {
                    singlePlayerElement.style.display = DisplayStyle.None;
                }

                m_Crosshair.Hide();
            }
            else
            {
                m_Crosshair.Show();
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (m_NetcodePanelPrefab != null)
            {
                Instantiate(m_NetcodePanelPrefab);
            }
#endif
        }

        public void UpdateLife(float life, float attackerPointerDegrees)
        {
            var displayStyle = DisplayStyle.None;
            if (life != m_HealthBar.value)
            {
                if (life < m_HealthBar.value)
                {
                    displayStyle = DisplayStyle.Flex;
                    m_AttackerPointerContainer.style.opacity = 1;
                    m_AttackerPointer.transform.rotation = quaternion.RotateZ(attackerPointerDegrees);
                    var opacity = Mathf.PingPong(Time.time * m_DamageIndicatorSpeed, m_DamageIndicatorMaxOpacity);
                    m_DamageIndicator.style.opacity = opacity;
                }

                m_HealthBar.value = life;
                m_HealthBarLabel.text = $"{(int)life}";
            }

            m_AttackerPointerContainer.style.display = displayStyle;
        }

        public void DisableDamageIndicator(float newLife)
        {
            if (m_DamageIndicator.style.opacity.value > 0 && newLife == m_HealthBar.value)
            {
                m_DamageIndicator.experimental.animation.Start(
                    new StyleValues { opacity = m_DamageIndicator.style.opacity.value },
                    new StyleValues { opacity = 0 }, 1000).OnCompleted(() =>
                {
                    m_AttackerPointerContainer.experimental.animation.Start(
                        new StyleValues { opacity = m_AttackerPointerContainer.style.opacity.value },
                        new StyleValues { opacity = 0 }, 1000);
                });
            }
        }

        public void ShowDeathMessage(string killerName)
        {
            FadeMessageScreen(true);
            m_MessageLabel.text = $"You have been destroyed by\n <color=#DF3D06> {killerName} </color>";
            StartCoroutine(DeathCooldown());
            m_Crosshair.Hide();
        }

        public void ShowBoundsMessage()
        {
            FadeMessageScreen(true);
            m_MessageLabel.text = "You still have unfinished business here";
            m_BottomMessageLabel.text = "";
            m_Crosshair.Hide();
        }

        private void FadeMessageScreen(bool value)
        {
            if (value)
            {
                m_MessageScreen.style.display = DisplayStyle.Flex;
                m_MessageScreen.experimental.animation
                    .Start(new StyleValues { opacity = 0f }, new StyleValues { opacity = 1f }, 1000);
            }
            else
            {
                m_MessageScreen.experimental.animation
                    .Start(new StyleValues { opacity = 1f }, new StyleValues { opacity = 0f }, 1000).OnCompleted(() =>
                    {
                        m_MessageScreen.style.display = DisplayStyle.None;
                    });
            }
        }

        private IEnumerator DeathCooldown()
        {
            var timer = m_DeathCooldown;
            while (timer >= 0)
            {
                var bottomMessage = (timer > 1)
                    ? $"Respawning in <color=#DF3D06> {math.trunc(timer)}</color>"
                    : "Returning...";
                m_BottomMessageLabel.text = bottomMessage;
                timer -= Time.deltaTime;
                yield return null;
            }
        }

        public void HideMessageScreen()
        {
            if (m_MessageScreen.style.display == DisplayStyle.Flex)
            {
                FadeMessageScreen(false);
                if (!PlayerInfoController.Instance.IsSinglePlayer)
                    m_Crosshair.Show();
            }
        }
        
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public void ToggleNetcodePanel()
        {
            NetcodePanelStats.Instance.ToggleNetcodePanel();
        }
#endif
    }
}