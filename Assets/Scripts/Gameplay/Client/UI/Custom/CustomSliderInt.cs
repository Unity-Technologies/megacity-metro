using UnityEngine.UIElements;

namespace MegacityMetro.CustomUI
{
    public class CustomSliderInt : SliderInt
    {
        public new class UxmlFactory : UxmlFactory<CustomSliderInt, UxmlTraits> { }

        public CustomSliderInt() : this(null) { }

        public CustomSliderInt(string label, int start = 0, int end = 10,
            SliderDirection direction = SliderDirection.Horizontal,
            float pageSize = 0.0f)
            : base(label, start, end, direction, pageSize)
        {
            AddToClassList("custom-slider");
            var dragger = this.Q("unity-dragger");
            var bar = new VisualElement();
            bar.AddToClassList("bar");
            dragger.Add(bar);
        }

        public override bool showInputField
        {
            get => base.showInputField;
            set
            {
                base.showInputField = value;
                if (showInputField)
                {
                    var textField = this.Q<TextField>("unity-text-field");
                    textField.Query().ForEach(e => { e.pickingMode = PickingMode.Ignore; });
                }
            }
        }
    }
}