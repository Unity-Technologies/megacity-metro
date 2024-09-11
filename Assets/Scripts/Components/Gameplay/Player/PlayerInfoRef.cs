﻿using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.MegacityMetro.Gameplay
{
    public struct PlayerInfoRef
    {
        public FixedString64Bytes Name;
        public Label Label;
        public float3 CurrentScale;
        public VisualElement Panel;
        public VisualElement PlayerInfo;
        public VisualElement Badge;
        public VisualElement StatusIcon;
        public VisualElement Icon;
        public ProgressBar LifeBar;
        public PlayerInfoItemSettings Settings;

        public PlayerInfoRef(float health, FixedString64Bytes playerName, VisualElement item,
            PlayerInfoItemSettings settings)
        {
            Label = item.Q<Label>("player-name");
            Badge = item.Q<VisualElement>("badge");
            LifeBar = item.Q<ProgressBar>("life-bar");
            Panel = item.Q<VisualElement>("player-info-panel");
            PlayerInfo = item.Q<VisualElement>("player-info");
            StatusIcon = item.Q<VisualElement>("state-icon");
            Icon = item.Q<VisualElement>("player-icon");
            Name = playerName;
            LifeBar.value = health;
            Settings = settings;
            CurrentScale = Vector3.one;
            Label.text = playerName.Value;
            SetChildrenUsageHint(LifeBar, UsageHints.DynamicTransform);
            InitializeStyles();
        }

        private void InitializeStyles()
        {
            StatusIcon.style.display = DisplayStyle.None;
            PlayerInfo.style.opacity = 0;
            Icon.style.opacity = 0;
        }

        private void SetChildrenUsageHint(VisualElement element, UsageHints usageHints)
        {
            if (element.childCount < 1)
                return;

            foreach (var child in element.Children())
            {
                child.usageHints = usageHints;
                SetChildrenUsageHint(child, usageHints);
            }
        }

        public void UpdateBadge(bool shouldHighlightTheName)
        {
            if (!Label.ClassListContains("highlight"))
                Label.AddToClassList("highlight");

            Label.EnableInClassList("highlight", shouldHighlightTheName);
            Badge.style.display = shouldHighlightTheName ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void UpdateLabel(FixedString64Bytes name)
        {
            if (!Name.Equals(name))
            {
                Name = name;
                Label.text = name.Value;
            }
        }

        public void SetLife(float life)
        {
            LifeBar.value = life;
            if (life <= 0 && StatusIcon.style.display == DisplayStyle.None)
            {
                LifeBar.style.display = DisplayStyle.None;
                Label.style.display = DisplayStyle.None;
                StatusIcon.style.display = DisplayStyle.Flex;
                StatusIcon.style.opacity = 1f;
            }
            else if (life > 0 && StatusIcon.style.display == DisplayStyle.Flex)
            {
                LifeBar.style.display = DisplayStyle.Flex;
                Label.style.display = DisplayStyle.Flex;
                StatusIcon.style.display = DisplayStyle.None;
                StatusIcon.style.opacity = 0f;
            }
        }

        public void UpdatePosition(float3 pos2D)
        {
            pos2D.x -= (Panel.contentRect.size.x / 2);
            pos2D.y -= Panel.contentRect.size.y / 2;
            Panel.transform.position = new Vector3(pos2D.x, pos2D.y, pos2D.z);
        }

        public void Hide()
        {
            Panel.style.display = DisplayStyle.None;
        }

        public void ShowPlayerInfo()
        {
            Panel.style.display = DisplayStyle.Flex;
            PlayerInfo.style.opacity = 1f;
            Icon.style.opacity = 0f;
        }

        public void ShowIcon()
        {
            Panel.style.display = DisplayStyle.Flex;
            PlayerInfo.style.opacity = 0f;
            Icon.style.opacity = 1f;
        }
        
        public bool IsVisible(float3 cameraPos, float3 cameraForward, float3 playerPos)
        {
            var cameraToPlayer = playerPos - cameraPos;
            var angle = math.degrees(math.acos(math.dot(math.normalize(cameraForward),
                math.normalize(cameraToPlayer))));
            // Check if the angle is within the threshold
            return angle < 45f;
        }

        public void UpdateIcon(float distance)
        {
            var value = math.clamp(distance / (Settings.MinDistanceToShowPlayerInfo * 2f), 0.25f, 1f);
            Icon.style.opacity = value;
            Icon.transform.scale = Vector3.one / (value * 4);
        }

        public void UpdateScale(float distance)
        {
            float scale;
            if (distance <= Settings.MinDistanceSq)
            {
                scale = Settings.MaxScale;
            }
            else if (distance >= Settings.MaxDistanceSq)
            {
                scale = Settings.MinScale;
            }
            else
            {
                var t = (distance - Settings.MinDistanceSq) / (Settings.MaxDistanceSq - Settings.MinDistanceSq);
                scale = math.lerp(Settings.MaxScale, Settings.MinScale, t);
            }

            CurrentScale.xyz = scale;
            Panel.transform.scale = CurrentScale;
        }
    }
}