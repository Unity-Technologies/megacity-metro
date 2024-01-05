using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

namespace Unity.NAME.Game
{
    // This file showcase useful examples of Attributes
    // It should not be included in the final build

    [RequireComponent(typeof(AudioListener)),  // Force the addition of the said component on the object the ExampleComponent is added
     DisallowMultipleComponent,                // Prevents having more than one ExampleComponent on the same GameObject
     ExecuteInEditMode,                        // Makes the component execute ONLY when the application is not running
     SelectionBase]                            // Will select this gameObject if the user selects a sub-gameObject in the editor
    public class ExampleComponent : MonoBehaviour
    {
        [Header("Leave you mouse cursor over the variables to get a tooltip!")]
        [Tooltip("This will be shown when you leave the mouse over the variable name")]
        public string MyCustomString = "Hello World!";

        [Space(10)] // To add space in the inspector (in pixel)

        // You can put more than one attribute on one line, separated with a comma
        // The Min or Max attribute can be used to limit the value
        [Min(0), Tooltip("This integer cannot go below 0")]
        public int MyMinimum;

        [Range(0f, 1f), Tooltip("This is a range to limit the value of the variable")]
        public float MyRange = 0.5f;

        [HideInInspector]
        public bool IsHidden = true;

        // Ability to apply attributes to the backing fields of auto-implemented properties, introduced in C# 7.3.
        // However, these look currently ugly in the Inspector ("<Serialized Property>k__Backing Field") so using
        // them is not recommended if the field is intented to be set in the Inspector on a regular basis. Useful
        // feature if wanting a field that can be set from the Inspector and from internal/protected/private code.
        // I believe people are working on making this look like a regular field in the Inspector.
        [field: SerializeField]
        public bool SerializedProperty { get; private set; }

        // Boilerplate code require for now to have the same functionality and appearance as above.
        public bool SerializedProperty2 { get => m_SerializedProperty2; private set => m_SerializedProperty2 = value; }
        [SerializeField] bool m_SerializedProperty2;

        [Multiline(2), Tooltip("This is a multiline string that shows more than one line")]
        public string MyMultiline = "This is the first line!\nThis is the second one!";

        [TextArea(3, 4), Tooltip("This is a text area")]
        public string MyTextArea = "This is a text area.\nYou can add more text.\nYou can even scroll down!\nThere we go!";

        [ColorUsage(showAlpha: true, hdr: true), Tooltip("MyColor uses the attribute ColorUsage to allow the alpha channel to be modified, and allow the HDR")]
        public Color MyColor = Color.white;

        [NonSerialized]
        public float MyNonSerializedPublicFloat;

        // Can also simply use properties.
        public float AlternativeNonSerializedPublicFloat { get; set; }

        [Tooltip("Custom class as serializable field")]
        public ExampleClass MyClass;

        [SerializeField, Tooltip("This is a private serialized field")]
        bool m_MyPrivateField = true;

        // If wanting to serialize a generic class in olrder versions, must derive a concrete subclass from a generic type.
        public ExampleIntGenericClass SerializedGenericWorkaround;

        // Serialisation of generic classes supported in 2020.1 and newer.
        public ExampleGenericClass<int> SerializedGenericClass;

        // Right click on the component to display the context functions
        [ContextMenu("My Context Function")]
        public void MyContextFunction()
        {
            Debug.Log($"Hello World! The value of m_MyPrivateField is {m_MyPrivateField}");
        }
    }

    [Serializable]
    public class ExampleClass
    {
        public bool MyBoolean;
    }

    [Serializable]
    public class ExampleGenericClass<T>
    {
        public string TestField;

        public ExampleGenericClass(T testValue)
        {
            TestField = testValue.ToString();
        }
    }

    [Serializable]
    public class ExampleIntGenericClass : ExampleGenericClass<int>
    {
        public ExampleIntGenericClass(int value) : base(value) {}
    }
}
