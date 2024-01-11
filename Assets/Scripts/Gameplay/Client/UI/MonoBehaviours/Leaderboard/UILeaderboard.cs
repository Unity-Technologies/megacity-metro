using UnityEngine;
using Unity.Collections;
using Unity.MegacityMetro.Gameplay;
using UnityEngine.UIElements;

namespace Unity.MegacityMetro.UI
{
    /// <summary>
    /// Leaderboard UI element
    /// </summary>
    public class UILeaderboard : MonoBehaviour
    {
        struct ScoreData
        {
            public float Value;
            public string Name;
            public bool IsOwnerPlayer;
            public bool HasData;
        }

        private enum VisibleMode { Full = 0, Collapse = 1 };

        [SerializeField]
        private int m_MaxList = 10;
        [SerializeField]
        private VisualTreeAsset m_LeaderboardItem;
        private VisualElement[] m_Items;
        private VisualElement m_Leaderboard;
        private Button m_BackButton;

        private ScoreData [] data;
        private VisibleMode m_VisibleMode = VisibleMode.Full;
        private int m_LastListSize = 0;

        private void Start()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            m_Leaderboard = root.Q<VisualElement>("leaderboard");
            m_BackButton = root.Q<Button>("info-panel-back-button");
            m_BackButton.clicked += Toggle;

            var leaderboardList = root.Q<VisualElement>("list-container");
            // there is an additional element in case the player is not in the top m_MaxList ranking
            m_Items = new VisualElement[m_MaxList + 1];
            data = new ScoreData[m_Items.Length];

            for (int i = 0; i < m_Items.Length; i++)
            {
                var item = m_LeaderboardItem.Instantiate();
                item.style.display = DisplayStyle.None;
                if (i == 0)
                {
                    //top player should be highlight
                    var nameLabel = item.Q<Label>("player-name");
                    item.AddToClassList("highlight");
                    nameLabel.AddToClassList("highlight");
                }
                leaderboardList.Add(item);
                m_Items[i] = item;
            }
        }

        public void Show()
        {
            m_Leaderboard.style.display = DisplayStyle.Flex;
        }

        public void Hide()
        {
            m_Leaderboard.style.display = DisplayStyle.None;
        }

        public void Toggle()
        {
            if(m_VisibleMode == VisibleMode.Full)
            {
                m_VisibleMode = VisibleMode.Collapse;
                for (int i = 1; i < m_Items.Length; i++)
                {
                    if (data[i].HasData && data[i].IsOwnerPlayer)
                        continue;
                    m_Items[i].style.display = DisplayStyle.None;
                }
            }
            else
            {
                m_VisibleMode = VisibleMode.Full;
                for (int i = 1; i < m_Items.Length; i++)
                {
                    if (data[i].HasData)
                        m_Items[i].style.display = DisplayStyle.Flex;
                }
            }
        }

        public void SetRanking(ref NativeArray<PlayerScoreAspect> leaderboard)
        {
            UpdateDataSetAndVisibility(leaderboard.Length);
            var isLocalPlayerInTheRanking = false;
            for (int i = 0; i < leaderboard.Length; i++)
            {
                // Iterate first top m_MaxList
                if (i < m_MaxList)
                {
                    if (leaderboard[i].Name.Equals(data[i].Name) &&
                        leaderboard[i].Value.Equals(data[i].Value) &&
                        data[i].HasData)
                        continue;

                    UpdateItem(leaderboard[i], ref isLocalPlayerInTheRanking, m_Items[i], i, i);
                }
                // If user is in inside in the top m_MaxList is going to hide the additional
                else if (isLocalPlayerInTheRanking)
                {
                    m_Items[m_MaxList].style.display = DisplayStyle.None;
                    break;
                }
                // Otherwise assign the user to the additional element.
                else if (leaderboard[i].IsOwnerPlayer)
                {
                    UpdateItem(leaderboard[i], ref isLocalPlayerInTheRanking, m_Items[m_MaxList], m_MaxList, i);
                    break;
                }
            }
        }

        private void UpdateDataSetAndVisibility(int currentSize)
        {
            if (m_LastListSize == currentSize)
                return;

            m_LastListSize = currentSize;
            var allowCollapseOption = m_LastListSize > 4;
            m_BackButton.style.display = allowCollapseOption ? DisplayStyle.Flex : DisplayStyle.None;
            if (!allowCollapseOption && m_VisibleMode == VisibleMode.Collapse)
            {
                Toggle();
            }

            for (int i = 0; i < m_Items.Length; i++)
            {
                m_Items[i].style.display = DisplayStyle.None;
                data[i].HasData = false;
            }
        }

        private void UpdateItem(PlayerScoreAspect player, ref bool isLocalPlayerInTheRanking, VisualElement item, int index, int pos)
        {
            data[index] = new ScoreData
            {
                IsOwnerPlayer = player.IsOwnerPlayer,
                Value = player.Value,
                Name = player.Name,
                HasData = true,
            };

            var nameLabel = item.Q<Label>("player-name");
            var rankLabel = item.Q<Label>("player-rank");
            var scoreLabel = item.Q<Label>("player-score");
            rankLabel.text = $"{pos+1}";
            nameLabel.text = $"{data[index].Name}";
            scoreLabel.text = data[index].Value.ToString("N0");

            // Owner player and top-ranked player are always be shown
            var isTopRanked = pos == 0;
            var shouldShow = item.style.display != DisplayStyle.Flex && m_VisibleMode == VisibleMode.Full;

            if (shouldShow || isTopRanked || player.IsOwnerPlayer)
                item.style.display = DisplayStyle.Flex;
            else if(item.style.display == DisplayStyle.Flex && m_VisibleMode == VisibleMode.Collapse)
                item.style.display = DisplayStyle.None;

            if (data[index].IsOwnerPlayer)
            {
                isLocalPlayerInTheRanking = true;
                item.AddToClassList("local-player");
            }
            else if (item.ClassListContains("local-player"))
            {
                item.RemoveFromClassList("local-player");
            }
        }
    }
}
