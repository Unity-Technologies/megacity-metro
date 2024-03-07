using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.MegacityMetro.Gameplay
{
    /// <summary>
    /// Manages the player name tags.
    /// </summary>
    public class PlayerInfoController : MonoBehaviour
    {
        public static PlayerInfoController Instance;
        
        [SerializeField]
        private PlayerInfoItemSettings m_Settings;
        [SerializeField]
        private VisualTreeAsset m_PlayerInfoItem;
        private VisualElement m_PlayerInfoContainer;
        private readonly Dictionary<Entity, PlayerInfoRef> NameTags = new();
        private Transform m_CameraTransform;
        private Camera m_Camera;

        public string PlayerName => m_Settings.PlayerName;
        public bool IsSinglePlayer => m_Settings.GameMode == GameMode.SinglePlayer;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                m_Camera = Camera.main;
                if (m_Camera != null) 
                    m_CameraTransform = m_Camera.transform;
            }
            else
            {
                Destroy(this);
            }
        }

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            m_PlayerInfoContainer = root.Q<VisualElement>("player-name-info-container");
        }

        public void CreateNameTag(string playerName, Entity player, float health)
        {
            if (NameTags.TryGetValue(player, out var nameTag))
            {
                var label = nameTag.Label;
                label.text = playerName;
            }
            else
            {
                var item = m_PlayerInfoItem.Instantiate();
                var playerInfo = new PlayerInfoRef(health, playerName, item, m_Settings);
                m_PlayerInfoContainer.Add(item);
                NameTags.Add(player, playerInfo);
            }
        }

        private void DestroyNameTag(Entity player)
        {
            m_PlayerInfoContainer.Remove(NameTags[player].Panel.parent);
            NameTags.Remove(player);
        }

        public void UpdateBadge(Entity player, bool shouldShow)
        {
            if (NameTags.TryGetValue(player, out var nameTag))
            {
                nameTag.UpdateBadge(shouldShow);
            }
        }

        public void UpdateNamePosition(Entity player, string playerName, float health, LocalToWorld localToWorld)
        {
            if (!NameTags.ContainsKey(player)) 
                return;
            
            NameTags[player].SetLife(health);
            var cameraPosition = m_CameraTransform.position;
            var cameraForward = m_CameraTransform.forward;
            var distance = math.distance(localToWorld.Position, cameraPosition);

            var screenPosition = m_Camera.WorldToScreenPoint(localToWorld.Position);
            if (screenPosition.z < 0 || !NameTags[player].IsVisible(cameraPosition, cameraForward,localToWorld.Position))
            {
                NameTags[player].Hide();
            }
            else
            {
                // Convert the screen position to panel position
                screenPosition = RuntimePanelUtils.ScreenToPanel(m_PlayerInfoContainer.panel, new Vector2(screenPosition.x, Screen.height - screenPosition.y));
                NameTags[player].UpdatePosition(screenPosition);
                NameTags[player].UpdateScale(distance);
                    
                if (distance < m_Settings.MinDistanceToShowPlayerInfo)
                {
                    NameTags[player].UpdateLabel(playerName);
                    if(!NameTags[player].InTransition && !NameTags[player].PlayerInfoVisible)
                        NameTags[player].ShowPlayerInfo();
                }
                else
                {
                    NameTags[player].UpdateIcon(distance);
                    if(!NameTags[player].InTransition && !NameTags[player].IconVisible)
                        NameTags[player].ShowIcon();
                }
            }
        }

        public void RefreshNameTags(EntityManager manager)
        {
            var list = new List<Entity>();
            foreach (var nameTag in NameTags.Keys)
            {
                if (!manager.Exists(nameTag))
                {
                    list.Add(nameTag);
                }
            }

            foreach (var entity in list)
            {
                DestroyNameTag(entity);
            }
        }

        public void SetMode(GameMode gameMode)
        {
            m_Settings.GameMode = gameMode;
        }

        public void ClearNames()
        {
            NameTags.Clear();
        }
    }
}