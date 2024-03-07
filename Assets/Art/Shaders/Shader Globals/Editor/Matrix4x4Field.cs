using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ShaderGlobals
{
    // ReSharper disable once InconsistentNaming
    class Matrix4x4Field : BaseField<Matrix4x4>
    {
        const string k_StyleGuid = "df117901ad6e4e8880649b4f290ba7ee";

        const string k_MatrixRowClass = "sg-matrix-row";
        const string k_MatrixElementClass = "sg-matrix-element";
        const string k_MatrixContainerName = "sg-matrix-container";

        FloatField[,] matrixElementFields { get; set; }
        readonly int m_Size;

        public override void SetValueWithoutNotify(Matrix4x4 newValue)
        {
            rawValue = newValue;

            for (var i = 0; i < m_Size; i++)
            {
                for (var j = 0; j < m_Size; j++)
                {
                    var v = newValue[i, j];
                    matrixElementFields[i, j].SetValueWithoutNotify(v);
                }
            }
        }

        public Matrix4x4Field()
            : base(null, null)
        {
            m_Size = 4;

            var container = new VisualElement();
            container.AddToClassList(k_MatrixContainerName);

            var stylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath(k_StyleGuid));
            if (stylesheet != null)
            {
                styleSheets.Add(stylesheet);
            }

            CreateMatrixElementUIs(container, 4);

            var input = this.Q(null, inputUssClassName);
            input.Add(container);
        }

        void CreateMatrixElementUIs(VisualElement container, int size)
        {
            matrixElementFields = new FloatField[size, size];

            for (var i = 0; i < size; i++)
            {
                var rowVisualElement = new VisualElement();
                rowVisualElement.AddToClassList(k_MatrixRowClass);

                for (var j = 0; j < size; j++)
                {
                    var v = value[i, j];
                    var fieldVisualElement = new FloatField {name = $"{k_MatrixElementClass}-{i}-{j}", value = v, isDelayed = true};

                    fieldVisualElement.AddToClassList(k_MatrixElementClass);
                    fieldVisualElement.RegisterValueChangedCallback(OnMatrixElementChanged);
                    fieldVisualElement.tooltip = $"Element {i}, {j}";

                    matrixElementFields[i, j] = fieldVisualElement;
                    rowVisualElement.Add(fieldVisualElement);
                }

                container.Add(rowVisualElement);
            }
        }

        void OnMatrixElementChanged(ChangeEvent<float> change)
        {
            change.StopPropagation();

            var newValue = new Matrix4x4();
            for (var i = 0; i < m_Size; i++)
            {
                for (var j = 0; j < m_Size; j++)
                {
                    newValue[i, j] = matrixElementFields[i, j].value;
                }
            }

            value = newValue;
        }

        /// <inheritdoc />
        protected override void UpdateMixedValueContent()
        {
            foreach (var field in matrixElementFields)
            {
                field.showMixedValue = showMixedValue;
            }
        }
    }
}
