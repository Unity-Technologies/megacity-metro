using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ShaderGlobals
{
    class ShaderGlobalsSettingsPage : VisualElement
    {
        const string k_TemplateGuid = "76ca053154d148beb5d26387dbf21cc2";
        const string k_StyleGuid = "62dbbb1fca6a4de7b2f3d96ab8d472be";

        static readonly Regex k_NameRemovePattern = new("^[^A-Za-z0-9_]+|[^A-Za-z0-9_]+$", RegexOptions.Compiled);
        static readonly Regex k_NameReplacePattern = new("[^A-Za-z0-9]+", RegexOptions.Compiled);

        MultiColumnListView m_List;

        SerializedObject m_Model;
        ShaderGlobals TargetObject => (ShaderGlobals) m_Model.targetObject;

        void OnTypeChanged(ChangeEvent<Enum> evt, int index)
        {
            var oldGlobal = TargetObject.shaderGlobals[index];
            var newGlobal = BaseShaderGlobal.Create((ShaderGlobalType) evt.newValue, oldGlobal.m_Name, oldGlobal.ObjectValue);

            m_Model.FindProperty(nameof(ShaderGlobals.shaderGlobals))
                .GetArrayElementAtIndex(index)
                .boxedValue = newGlobal;

            m_Model.ApplyModifiedProperties();
            m_List.RefreshItem(index);
        }

        void OnNameChanged(ChangeEvent<string> evt, int index)
        {
            var newName = SanitizeName(evt.newValue, index);
            (evt.target as TextField)?.SetValueWithoutNotify(newName);

            m_Model.FindProperty(nameof(ShaderGlobals.shaderGlobals))
                .GetArrayElementAtIndex(index)
                .FindPropertyRelative(nameof(BaseShaderGlobal.m_Name))
                .stringValue = newName;

            m_Model.ApplyModifiedProperties();
        }

        string SanitizeName(string referenceName, int index = -1)
        {
            var sanitizedName = k_NameRemovePattern.Replace(referenceName, "");
            sanitizedName = k_NameReplacePattern.Replace(sanitizedName, "_");

            if (sanitizedName.Length < 1 || char.IsDigit(sanitizedName[0]))
                sanitizedName = "_" + sanitizedName;

            var existingNames = new HashSet<string>();
            for (var i = 0; i < TargetObject.shaderGlobals.Count; i++)
            {
                if (i == index || TargetObject.shaderGlobals[i] == null)
                    continue;

                existingNames.Add(TargetObject.shaderGlobals[i].m_Name);
            }

            if (!existingNames.Contains(sanitizedName))
            {
                return sanitizedName;
            }

            var suffix = 1;
            while (existingNames.Contains($"{sanitizedName}_{suffix}"))
            {
                suffix++;
            }

            return $"{sanitizedName}_{suffix}";
        }

        public ShaderGlobalsSettingsPage(SerializedObject shaderGlobals)
        {
            m_Model = shaderGlobals;

            var template = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AssetDatabase.GUIDToAssetPath(k_TemplateGuid));
            template.CloneTree(this);

            AddToClassList("sg-shader-globals");
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath(k_StyleGuid));
            styleSheets.Add(styleSheet);

            m_List = this.Q<MultiColumnListView>("globals-list");
            m_List.itemsSource = TargetObject.shaderGlobals;

            m_List.itemsAdded += itemsAdded =>
            {
                foreach (var item in itemsAdded)
                {
                    TargetObject.shaderGlobals[item] = BaseShaderGlobal.Create(ShaderGlobalType.Float, SanitizeName("NewGlobal"));
                }
                m_Model.Update();
            };

            m_List.itemsRemoved += itemsRemoved =>
            {
                foreach (var item in itemsRemoved)
                {
                    TargetObject.shaderGlobals[item].Reset();
                }
                m_Model.Update();
            };

            m_List.itemIndexChanged += (oldIndex, newIndex) =>
            {
                m_Model.Update();
                m_List.RefreshItem(oldIndex);
                m_List.RefreshItem(newIndex);
            };

            m_List.TrackPropertyValue(m_Model.FindProperty(nameof(ShaderGlobals.shaderGlobals)), _ =>
            {
                m_List.Rebuild();
            });

            m_List.columns["type-column"].makeCell = () => new EnumField(ShaderGlobalType.Float);
            m_List.columns["type-column"].bindCell = (v, i) =>
            {
                var field = (EnumField) v;
                field.SetValueWithoutNotify(TargetObject.shaderGlobals[i].Type);
                field.RegisterCallback<ChangeEvent<Enum>, int>(OnTypeChanged, i);
            };

            m_List.columns["type-column"].unbindCell = (v, _) =>
            {
                var field = (EnumField) v;
                field.UnregisterCallback<ChangeEvent<Enum>, int>(OnTypeChanged);
            };

            m_List.columns["name-column"].makeCell = () => new TextField {isDelayed = true};
            m_List.columns["name-column"].bindCell = (v, i) =>
            {
                var field = (TextField) v;
                field.SetValueWithoutNotify(TargetObject.shaderGlobals[i].m_Name);
                field.RegisterCallback<ChangeEvent<string>, int>(OnNameChanged, i);
                field.TrackPropertyValue(m_Model.FindProperty(nameof(ShaderGlobals.shaderGlobals)).GetArrayElementAtIndex(i).FindPropertyRelative(nameof(BaseShaderGlobal.m_Name)), _ =>
                {
                    field.SetValueWithoutNotify(TargetObject.shaderGlobals[i].m_Name);
                });
            };

            m_List.columns["name-column"].unbindCell = (v, _) =>
            {
                var field = (TextField) v;
                field.UnregisterCallback<ChangeEvent<string>, int>(OnNameChanged);
            };

            m_List.columns["value-column"].makeCell = () =>
            {
                var cell = new ShaderGlobalValueCell();
                cell.AddToClassList("sg-shader-globals__value-field");
                return cell;
            };

            m_List.columns["value-column"].bindCell = (v, i) =>
            {
                var element = shaderGlobals
                    .FindProperty(nameof(ShaderGlobals.shaderGlobals))
                    .GetArrayElementAtIndex(i);

                var cell = (ShaderGlobalValueCell) v;
                cell.Build(element);
            };

            m_List.columns["value-column"].unbindCell = (v, _) =>
            {
                var cell = (ShaderGlobalValueCell) v;
                cell.Reset();
            };
        }
    }
}
