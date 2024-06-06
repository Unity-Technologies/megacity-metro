using UnityEngine.UIElements;

namespace MegacityMetro.CustomUI
{
    public class CustomSlider : Slider
    {
        public new class UxmlFactory : UxmlFactory<CustomSlider, UxmlTraits> { }

        public CustomSlider() : this(null) { }

        public CustomSlider(string label, float start = 0.0f, float end = 10f,
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