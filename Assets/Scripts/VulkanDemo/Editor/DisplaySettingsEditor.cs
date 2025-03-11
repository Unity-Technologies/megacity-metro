using UnityEditor;

[CustomEditor(typeof(DisplaySettings))]
[CanEditMultipleObjects]
public class DisplaySettingsEditor : Editor
{
    SerializedProperty targetFramerate, keepScreenAwake, dynamicResolutionControls;

    void OnEnable()
    {
        targetFramerate = serializedObject.FindProperty("targetFramerate");
        keepScreenAwake = serializedObject.FindProperty("keepScreenAwake");
        dynamicResolutionControls = serializedObject.FindProperty("dynamicResolutionControls");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var drType = dynamicResolutionControls.enumValueIndex;

        EditorGUILayout.PropertyField(keepScreenAwake);

        if (drType == 2)
            EditorGUILayout.LabelField("Framerate is controlled by Adaptive Performance settings when active");
        else
            EditorGUILayout.PropertyField(targetFramerate);

        EditorGUILayout.PropertyField(dynamicResolutionControls);

        if (drType > 0)
        {
            EditorGUILayout.LabelField("Ensure that Dynamic Resolution is enabled on the camera");
            if (drType == 2)
                EditorGUILayout.LabelField("Ensure that Adaptive Performance is enabled in Project Settings");
        }

        serializedObject.ApplyModifiedProperties();
    }

}