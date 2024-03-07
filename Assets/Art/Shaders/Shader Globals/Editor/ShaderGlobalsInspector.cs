using UnityEditor;
using UnityEngine.UIElements;

namespace ShaderGlobals
{
    [CustomEditor(typeof(ShaderGlobals))]
    public class ShaderGlobalsInspector : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            return new ShaderGlobalsSettingsPage(serializedObject);
        }
    }
}
