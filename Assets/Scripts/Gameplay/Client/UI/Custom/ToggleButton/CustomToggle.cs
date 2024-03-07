using System.Collections.Generic;
using UnityEngine.UIElements;

namespace MegacityMetro.CustomUI
{
    public class CustomToggle : BaseField<bool>
    {
        public new class UxmlFactory : UxmlFactory<CustomToggle, UxmlTraits>
        {
        }

        public new class UxmlTraits : BaseFieldTraits<bool, UxmlBoolAttributeDescription>
        {
            UxmlStringAttributeDescription m_FirstOptionText = new() {name = "first-option"};
            UxmlStringAttributeDescription m_SecondOptionText = new() {name = "second-option"};
            
            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }
            
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                ((CustomToggle)ve).FirstOption = m_FirstOptionText.GetValueFromBag(bag, cc);
                ((CustomToggle)ve).SecondOption = m_SecondOptionText.GetValueFromBag(bag, cc);
            }
        }

        public static readonly new string ussClassName = "custom-toggle";
        public static readonly new string inputUssClassName = "custom-toggle__input";
        public static readonly string inputOptionUssClassName = "custom-toggle__input-option";
        public static readonly string inputOptionCheckedUssClassName = "custom-toggle__input-option--checked";

        private VisualElement m_Input;
        private Label m_FirstOption;
        private Label m_SecondOption;

        public string FirstOption
        {
            get => m_FirstOption.text;
            set => m_FirstOption.text = value;
        }
        public string SecondOption
        {
            get => m_SecondOption.text;
            set => m_SecondOption.text = value;
        }

        public CustomToggle() : this(null)
        {
        }

        public CustomToggle(string label) : base(label, null)
        {
            AddToClassList(ussClassName);

            m_Input = this.Q(className: BaseField<bool>.inputUssClassName);
            m_Input.AddToClassList(inputUssClassName);
            Add(m_Input);

            m_FirstOption = new Label();
            m_FirstOption.AddToClassList(inputOptionUssClassName);
            m_FirstOption.AddToClassList(inputOptionCheckedUssClassName);
            m_FirstOption.EnableInClassList(inputOptionCheckedUssClassName, false);
            m_Input.Add(m_FirstOption);

            var separator = new Label("/");
            m_Input.Add(separator);

            m_SecondOption = new Label();
            m_SecondOption.AddToClassList(inputOptionUssClassName);
            m_SecondOption.AddToClassList(inputOptionCheckedUssClassName);
            m_Input.Add(m_SecondOption);

            RegisterCallback<ClickEvent>(OnClick);
            RegisterCallback<NavigationSubmitEvent>(OnSubmit);
        }

        private static void OnClick(ClickEvent evt)
        {
            var customToggle = evt.currentTarget as CustomToggle;
            customToggle.ToggleValue();
            evt.StopPropagation();
        }

        private static void OnSubmit(NavigationSubmitEvent evt)
        {
            var customToggle = evt.currentTarget as CustomToggle;
            customToggle.ToggleValue();
            evt.StopPropagation();
        }

        private void ToggleValue()
        {
            value = !value;
            m_FirstOption.EnableInClassList(inputOptionCheckedUssClassName, value);
            m_SecondOption.EnableInClassList(inputOptionCheckedUssClassName, !value);
        }

        public override void SetValueWithoutNotify(bool newValue)
        {
            base.SetValueWithoutNotify(newValue);
            m_Input.EnableInClassList(inputOptionUssClassName, newValue);
            m_FirstOption.EnableInClassList(inputOptionCheckedUssClassName, newValue);
            m_SecondOption.EnableInClassList(inputOptionCheckedUssClassName, !newValue);
        }

        public override bool value
        {
            get => base.value;
            set
            { 
                base.value = value;
                m_Input.EnableInClassList(inputOptionUssClassName, value);
                m_FirstOption.EnableInClassList(inputOptionCheckedUssClassName, value);
                m_SecondOption.EnableInClassList(inputOptionCheckedUssClassName, !value);
            }
        }
    }
}