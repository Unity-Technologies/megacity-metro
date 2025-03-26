using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.MegacityMetro.UGS;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UIElements;

public class SessionBrowser : MonoBehaviour
{
    private VisualElement m_SessionBrowser;
    private ListView m_SessionList;
    private Button m_RefreshButton;
    private Button m_PlayButton;

    public VisualTreeAsset SessionListItemAsset;

    private List<ISessionInfo> m_Sessions = new();

    private class SessionItem : VisualElement
    {
        public SessionItem(VisualTreeAsset sessionTemplate)
        {
            var sessionItemInstance = sessionTemplate.Instantiate();
            Add(sessionItemInstance);
            var sessionItem = sessionItemInstance.Q<VisualElement>("session-item");
            SessionName = sessionItem.Q<Label>("session-name");
            PlayerCount = sessionItem.Q<Label>("player-count");
            PlayerMax = sessionItem.Q<Label>("player-max");
        }

        public Label SessionName;
        public Label PlayerCount;
        public Label PlayerMax;
    }

    VisualElement MakeItem() => new SessionItem(SessionListItemAsset);

    void BindItem(VisualElement e, int i)
    {
        if (e is not SessionItem sessionItem)
            return;
        var sessionInfo = m_Sessions[i];
        sessionItem.SessionName.text = sessionInfo.Id;
        sessionItem.PlayerCount.text = (sessionInfo.MaxPlayers - sessionInfo.AvailableSlots).ToString();
        sessionItem.PlayerMax.text = sessionInfo.MaxPlayers.ToString();
    }

    private void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        m_SessionBrowser = root.Q<VisualElement>("session-browser");
        m_SessionList = m_SessionBrowser.Q<ListView>("session-list");
        m_RefreshButton = root.Q<Button>("session-refresh-button");
        m_PlayButton = root.Q<Button>("multiplayer-play-button");
        m_SessionList.makeItem = MakeItem;
        m_SessionList.bindItem = BindItem;
        m_SessionList.itemsSource = m_Sessions;
        m_SessionList.selectionType = SelectionType.Single;
        m_SessionList.fixedItemHeight = 40;

        // Prevent navigation inside the multiplayer form to avoid issue with the TextField
        m_SessionList.RegisterCallback<NavigationMoveEvent>(evt =>
        {
            evt.StopPropagation();
            if (evt.target is VisualElement element) { element.focusController.IgnoreEvent(evt); }
        }, TrickleDown.TrickleDown);

        m_SessionList.itemsChosen += async (selectedItems) =>
        {
            if (selectedItems.FirstOrDefault() is not ISessionInfo sessionInfo)
                return;
            await MatchMakingConnector.Instance.JoinSession(sessionInfo.Id);
        };

        Task.Run(async () =>
        {
            await RefreshSessionList();
        });
        m_RefreshButton.clicked += async () =>
        {
            await RefreshSessionList();
        };

        m_PlayButton.clicked += async () =>
        {
            if (m_SessionList.selectedItem is not ISessionInfo sessionInfo)
                return;
            await MatchMakingConnector.Instance.JoinSession(sessionInfo.Id);
        };
    }

    private async Task RefreshSessionList()
    {
        m_Sessions.Clear();
        m_Sessions.AddRange(await MatchMakingConnector.Instance.ListSessions());
        m_SessionList.RefreshItems();
    }
}
