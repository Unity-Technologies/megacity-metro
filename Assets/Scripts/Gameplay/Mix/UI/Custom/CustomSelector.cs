using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace MegacityMetro.CustomUI
{
    public class CustomSelector : BaseField<string>
    {
        public new class UxmlFactory : UxmlFactory<CustomSelector, UxmlTraits> { }

        public new class UxmlTraits : BaseFieldTraits<string, UxmlStringAttributeDescription>
        {
            private UxmlIntAttributeDescription m_Index;
            private UxmlStringAttributeDescription m_Choices;

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                CustomSelector customSelector = (CustomSelector)ve;
                List<string> choiceList = ParseChoiceList(m_Choices.GetValueFromBag(bag, cc));
                if (choiceList != null)
                    customSelector.choices = choiceList;
                customSelector.index = m_Index.GetValueFromBag(bag, cc);
            }

            public UxmlTraits()
            {
                UxmlIntAttributeDescription attributeDescription1 = new UxmlIntAttributeDescription
                {
                    name = "index",
                    defaultValue = -1
                };
                m_Index = attributeDescription1;

                UxmlStringAttributeDescription attributeDescription2 = new UxmlStringAttributeDescription
                {
                    name = "choices"
                };
                m_Choices = attributeDescription2;
            }

            private static List<string> ParseChoiceList(string choicesFromBag)
            {
                if (string.IsNullOrEmpty(choicesFromBag.Trim()))
                    return null;
                string[] strArray = choicesFromBag.Split(',');
                if (strArray.Length == 0)
                    return null;
                List<string> choiceList = new List<string>();
                foreach (string str in strArray)
                    choiceList.Add(str.Trim());
                return choiceList;
            }
        }

        private new static readonly string ussClassName = "custom-selector";
        private static readonly string textElementUssClassName = "custom-selector__text";
        private static readonly string fieldContainerUssClassName = "custom-selector__container";
        private static readonly string arrowButtonUssClassName = "custom-selector__arrow-button";
        private static readonly string leftArrowImageUssClassName = "custom-selector__left-arrow";
        private static readonly string rightArrowImageUssClassName = "custom-selector__right-arrow";

        private int m_Index;
        private List<string> m_Choices;
        private TextElement m_TextElement;

        public CustomSelector() : this(null, null)
        {
        }

        public CustomSelector(string label, VisualElement visualInput) : base(label, visualInput)
        {
            AddToClassList(ussClassName);

            VisualElement container = new VisualElement { name = "container" };
            container.AddToClassList(fieldContainerUssClassName);
            Add(container);

            VisualElement leftButton = new VisualElement { name = "left-button" };
            leftButton.AddToClassList(arrowButtonUssClassName);
            leftButton.AddManipulator(new Clickable(OnLeftButtonClicked));
            container.Add(leftButton);

            var leftArrow = new VisualElement { name = "left-arrow" };
            leftArrow.AddToClassList(leftArrowImageUssClassName);
            leftButton.Add(leftArrow);

            VisualElement inputField = this.Q(className: inputUssClassName);
            inputField.name = "input";
            container.Add(inputField);

            m_TextElement = new TextElement { name = "text" };
            m_TextElement.AddToClassList(textElementUssClassName);
            inputField.Add(m_TextElement);

            VisualElement rightButton = new VisualElement { name = "right-button" };
            rightButton.AddToClassList(arrowButtonUssClassName);
            rightButton.AddManipulator(new Clickable(OnRightArrowClicked));
            container.Add(rightButton);

            var rightArrow = new VisualElement { name = "right-arrow" };
            rightArrow.AddToClassList(rightArrowImageUssClassName);
            rightButton.Add(rightArrow);

            RegisterCallback<NavigationMoveEvent>(OnNavigationMove);
        }

        private void OnNavigationMove(NavigationMoveEvent evt)
        {
            if (evt.direction == NavigationMoveEvent.Direction.Left)
                OnLeftButtonClicked(evt);
            else if (evt.direction == NavigationMoveEvent.Direction.Right)
                OnRightArrowClicked(evt);
        }

        private void OnRightArrowClicked(EventBase obj)
        {
            if (m_Index >= m_Choices.Count - 1)
                return;
            
            ++m_Index;
            value = m_Choices[m_Index];
        }

        private void OnLeftButtonClicked(EventBase evt)
        {
            if (m_Index <= 0)
                return;
            
            --m_Index;
            value = m_Choices[m_Index];
        }

        public virtual List<string> choices
        {
            get => m_Choices;
            set
            {
                m_Choices = value ?? throw new ArgumentNullException(nameof(value));
                SetValueWithoutNotify(this.value);
            }
        }

        public int index
        {
            get => m_Index;
            set
            {
                if (value == m_Index)
                    return;
                m_Index = value;
                if (m_Index >= 0 && m_Index < m_Choices.Count)
                    this.value = m_Choices[m_Index];
                else
                    this.value = default;
            }
        }
        
        public override string value
        {
            get => base.value;
            set
            {
                base.value = value;
                m_TextElement.text = value;
                m_Index = m_Choices.IndexOf(value);
            }
        }
    }
}