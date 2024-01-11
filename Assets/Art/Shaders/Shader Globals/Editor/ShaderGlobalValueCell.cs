using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ShaderGlobals
{
    // Displays the appropriate editor for the value of a given BaseShaderGlobal as a SerializedProperty.
    // Having a wrapper VisualElement for this makes use in a MultiColumnListView easier.
    class ShaderGlobalValueCell : VisualElement
    {
        VisualElement m_Field;

        static VisualElement BindField<TField>(TField field, SerializedProperty valueProp) where TField : VisualElement, IBindable
        {
            field.BindProperty(valueProp);
            return field;
        }

        static Matrix4x4 ReadMatrix(SerializedProperty sp)
        {
            var value = new Matrix4x4();

            for (var row = 0; row < 4; row++)
            {
                for (var col = 0; col < 4; ++col)
                {
                    value[row, col] = sp.FindPropertyRelative($"e{row}{col}").floatValue;
                }
            }

            return value;
        }

        static void WriteMatrix(SerializedProperty sp, Matrix4x4 value)
        {
            for (var row = 0; row < 4; row++)
            {
                for (var col = 0; col < 4; ++col)
                {
                    sp.FindPropertyRelative($"e{row}{col}").floatValue = value[row, col];
                }
            }
        }

        static VisualElement CreateMatrixField(SerializedProperty valueProp)
        {
            var matrixField = new Matrix4x4Field();

            matrixField.SetValueWithoutNotify(ReadMatrix(valueProp));

            matrixField.TrackSerializedObjectValue(valueProp.serializedObject, (so) =>
            {
                var newProp = so.FindProperty(valueProp.propertyPath);
                if (newProp is null)
                {
                    return;
                }

                matrixField.SetValueWithoutNotify(ReadMatrix(valueProp));
            });

            matrixField.RegisterValueChangedCallback(evt =>
            {
                WriteMatrix(valueProp, evt.newValue);
                valueProp.serializedObject.ApplyModifiedProperties();
            });

            return matrixField;
        }

        static VisualElement MakeEditor(SerializedProperty shaderGlobalProp)
        {
            if (shaderGlobalProp?.boxedValue is not BaseShaderGlobal shaderGlobal)
            {
                return new VisualElement();
            }

            var valueType = shaderGlobal.Type;
            var valueProp = shaderGlobalProp.FindPropertyRelative(nameof(ShaderGlobal<float>.m_Value));

            return valueType switch
            {
                ShaderGlobalType.Float => BindField(new FloatField(), valueProp),
                ShaderGlobalType.Integer => BindField(new IntegerField(), valueProp),
                ShaderGlobalType.Color => BindField(new ColorField(), valueProp),
                ShaderGlobalType.Texture => BindField(new ObjectField {objectType = typeof(Texture)}, valueProp),
                ShaderGlobalType.Vector => BindField(new Vector4Field(), valueProp),
                ShaderGlobalType.Matrix => CreateMatrixField(valueProp),

                _ => new Label($"Unknown Shader Global type: {valueType}"),
            };
        }

        public void Build(SerializedProperty shaderGlobalProp)
        {
            if (m_Field != null) Reset();

            m_Field = MakeEditor(shaderGlobalProp);
            Add(m_Field);
        }

        public void Reset()
        {
            m_Field?.Unbind();
            Clear();
        }
    }
}
