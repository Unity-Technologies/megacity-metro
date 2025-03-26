using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MegacityMetro.CustomUI;
using Unity.MegacityMetro.Gameplay;
using Unity.MegacityMetro.UGS;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.MegacityMetro.UI
{
    public class MultiplayerMenu : MainMenuGameMode
    {
        public enum MultiplayerMode
        {
            Matchmaker = 0,
            Connect = 1
        }

        [field: SerializeField]
        public MultiplayerMode SelectedMultiplayerMode { get; private set; } = MultiplayerMode.Matchmaker;

        [SerializeField]
        private PlayerInfoItemSettings m_PlayerSettings;

        // UI Elements
        private VisualElement m_MultiplayerMenuOptions;
        private VisualElement m_MultiplayerForm;
        private Button m_MultiplayerPlayButton;
        private TextField m_NameTextField;
        private CustomToggle m_MultiplayerModeGroup;
        private Button m_MultiplayerReturnButton;
        protected override GameMode GameMode => GameMode.Multiplayer;
        protected override VisualElement m_MenuOptions => m_MultiplayerMenuOptions;
        protected override VisualElement m_AutomaticFocusElement => m_MultiplayerModeGroup;
        public static MultiplayerMenu Instance { get; private set; }
        private GameInput m_GameInput;
        private CustomFocusRing m_FocusRing;
        private VisualElement m_ActionButtonStrip;
        private Button m_RefreshButton;
        private VisualElement m_SessionBrowser;

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

        private void OnNavigate(Vector2 direction)
        {
            if (m_FocusRing == null || m_MultiplayerMenuOptions.ClassListContains(UIConstants.k_HiddenUssElementClass))
                return;

            switch (direction.y)
            {
                case > 0:
                    m_FocusRing.Previous();
                    break;
                case < 0:
                    m_FocusRing.Next();
                    break;
            }
        }

        public override void InitUI()
        {
            base.InitUI();

            // Initialize MatchMakingConnector
            MatchMakingUI.Instance.Init();

            // Get references to UI elements
            var root = GetComponent<UIDocument>().rootVisualElement;
            m_MultiplayerMenuOptions = root.Q<VisualElement>("multiplayer-menu-options");
            m_MultiplayerModeGroup = m_MultiplayerMenuOptions.Q<CustomToggle>("multiplayer-mode");
            m_NameTextField = m_MultiplayerMenuOptions.Q<TextField>("name-textfield");
            m_MultiplayerPlayButton = m_MultiplayerMenuOptions.Q<Button>("multiplayer-play-button");
            m_MultiplayerReturnButton = m_MultiplayerMenuOptions.Q<Button>("multiplayer-return-button");
            m_SessionBrowser = root.Q<VisualElement>("session-browser");
            m_NameTextField.SetValueWithoutNotify(m_PlayerSettings.PlayerName);

            m_RefreshButton = root.Q<Button>("session-refresh-button");

            m_ActionButtonStrip = root.Q<VisualElement>("multiplayer-button-strip");

            DropdownField serverDropDown = m_MultiplayerMenuOptions.Q<DropdownField>("multiplayer-server-location");

            // Create custom ring for focus navigation
#if UNITY_SWITCH
            m_FocusRing = new CustomFocusRing(root, new VisualElement[] {m_MultiplayerModeGroup, serverDropDown, m_MultiplayerPlayButton, m_MultiplayerReturnButton});
#else
            m_FocusRing = new CustomFocusRing(root,
                new VisualElement[]
                {
                    m_MultiplayerModeGroup, serverDropDown, m_NameTextField, m_MultiplayerPlayButton,
                    m_MultiplayerReturnButton
                });
#endif

#if (UNITY_ANDROID || UNITY_IPHONE || UNITY_SWITCH) && !DEVELOPMENT_BUILD && !UNITY_EDITOR
            m_MultiplayerModeGroup.AddToClassList(UIConstants.k_HiddenUssElementClass);
#endif
#if UNITY_SWITCH
            m_NameTextField.isReadOnly = true;
#endif
            m_MultiplayerMenuOptions.AddToClassList(UIConstants.k_HiddenUssElementClass);
            m_MultiplayerModeGroup.RegisterValueChangedCallback(evt =>
            {
                SetConnectionMode((MultiplayerMode)Convert.ToInt32(evt.newValue));
            });

            m_NameTextField.RegisterValueChangedCallback(evt =>
            {
                var filteredText = FilterNonAlphanumeric(evt.newValue);
                if (filteredText != evt.newValue)
                    m_NameTextField.SetValueWithoutNotify(filteredText);

                m_NameTextField.SetValueWithoutNotify(m_NameTextField.text);
            });

            m_MultiplayerReturnButton.clicked += BackToTheMenu;

            serverDropDown.RegisterCallback<GeometryChangedEvent>(_ =>
            {
                SetConnectionMode(SelectedMultiplayerMode);
            });

            // Subscribe to UI events
            UIEvents.OnNavigate += OnNavigate;

            // Prevent navigation inside the multiplayer form to avoid issue with the TextField
            m_MultiplayerMenuOptions.RegisterCallback<NavigationMoveEvent>(evt =>
            {
                evt.StopPropagation();
                evt.PreventDefault();
            }, TrickleDown.TrickleDown);

            MatchMakingUI.Instance.Connecting += () => MainMenu.Instance.CurrentState = MenuState.Matchmaking;
        }

        /// <summary>
        /// Filter out non-alphanumeric characters from the input string
        /// </summary>
        /// <param name="input"> Input string to filter</param>
        /// <returns> Filtered string</returns>
        private string FilterNonAlphanumeric(string input)
        {
            return Regex.Replace(input, "[^a-zA-Z0-9-_]", string.Empty);
        }

        private void SetConnectionMode(MultiplayerMode mode)
        {
            SelectedMultiplayerMode = mode;
            var isMatchMaking = mode == MultiplayerMode.Matchmaker;

            var connectButtonText = isMatchMaking
                ? "Find Match"
                : nameof(MultiplayerMode.Connect);

            var sessionList = m_SessionBrowser.Q<ListView>();

            m_ActionButtonStrip.EnableInClassList("button-strip__wide", !isMatchMaking);
            m_ActionButtonStrip.EnableInClassList("button-strip__normal", isMatchMaking);
            m_RefreshButton.EnableInClassList(UIConstants.k_HiddenUssElementClass, isMatchMaking);
            m_SessionBrowser.EnableInClassList("hidden", isMatchMaking);

            m_MultiplayerPlayButton.text = connectButtonText;
            m_MultiplayerPlayButton.SetEnabled(isMatchMaking || sessionList.selectedItem != null);

            sessionList.selectionType = SelectionType.Single;

            // callback to enable play button when list has a selection
            sessionList.selectionChanged -= OnSessionSelectionChanged;
            sessionList.selectionChanged += OnSessionSelectionChanged;

            // This should occur after Matchmaking assigns the random name.
            m_NameTextField.value = m_PlayerSettings.PlayerName;

            // Setting matchmaker UI conditions
            MatchMakingUI.Instance.SetConnectionMode(isMatchMaking);
            return;

            void OnSessionSelectionChanged(IEnumerable<object> selections)
            {
                var playEnabled =
                    (selections != null && selections.Any())
                    || (isMatchMaking || sessionList.selectedItem != null);

                m_MultiplayerPlayButton.SetEnabled(playEnabled);
            }
        }

        protected override void BackToTheMenu()
        {
            base.BackToTheMenu();
            MatchMakingConnector.Instance.ClearSession();
        }
    }
}
