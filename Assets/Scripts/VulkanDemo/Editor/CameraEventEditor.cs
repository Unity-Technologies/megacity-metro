using UnityEditor;

[CustomEditor(typeof(CameraEvent))]
[CanEditMultipleObjects]
public class CameraEventEditor : Editor
{
    SerializedProperty focusPointBool, focusPointValue, focusPointTime;
    SerializedProperty distanceBool, distanceValue, distanceTime;
    SerializedProperty yawBool, yawValue, yawTime;
    SerializedProperty pitchBool, pitchValue, pitchTime;

    void OnEnable()
    {
        focusPointBool = serializedObject.FindProperty("modifyFocusTarget");
        focusPointValue = serializedObject.FindProperty("focusTargetIndex");
        focusPointTime = serializedObject.FindProperty("focusTargetTransitionTime");
        distanceBool = serializedObject.FindProperty("modifyDistance");
        distanceValue = serializedObject.FindProperty("newDistance");
        distanceTime = serializedObject.FindProperty("distanceTransitionSpeed");
        yawBool = serializedObject.FindProperty("modifyYaw");
        yawValue = serializedObject.FindProperty("newYaw");
        yawTime = serializedObject.FindProperty("yawTransitionSpeed");
        pitchBool = serializedObject.FindProperty("modifyPitch");
        pitchValue = serializedObject.FindProperty("newPitch");
        pitchTime = serializedObject.FindProperty("pitchTransitionSpeed");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(focusPointBool);
        if (focusPointBool.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(focusPointValue);
            EditorGUILayout.PropertyField(focusPointTime);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.PropertyField(distanceBool);
        if (distanceBool.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(distanceValue);
            EditorGUILayout.PropertyField(distanceTime);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.PropertyField(yawBool);
        if (yawBool.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(yawValue);
            EditorGUILayout.PropertyField(yawTime);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.PropertyField(pitchBool);
        if (pitchBool.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(pitchValue);
            EditorGUILayout.PropertyField(pitchTime);
            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }

}
