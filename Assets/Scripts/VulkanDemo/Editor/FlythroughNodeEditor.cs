using UnityEditor;

[CustomEditor(typeof(FlythroughNode))]
[CanEditMultipleObjects]
public class FlythroughNodeEditor : Editor
{

    SerializedProperty entryCurveStrengthProp, exitCurveStrengthProp;
    SerializedProperty modifiesSpeedProp, newSpeedProp, speedTransitionTimeProp;
    SerializedProperty modifiesAngleProp, newAngleProp, angleTransitionTimeProp;
    SerializedProperty cameraEventProp;
    SerializedProperty actionTriggersProp;

    void OnEnable()
    {
        entryCurveStrengthProp = serializedObject.FindProperty("entryCurveStrength");
        exitCurveStrengthProp = serializedObject.FindProperty("exitCurveStrength");
        modifiesSpeedProp = serializedObject.FindProperty("modifyShipSpeed");
        newSpeedProp = serializedObject.FindProperty("speed");
        speedTransitionTimeProp = serializedObject.FindProperty("timeToSpeed");
        modifiesAngleProp = serializedObject.FindProperty("modifyShipAngle");
        newAngleProp = serializedObject.FindProperty("roll");
        angleTransitionTimeProp = serializedObject.FindProperty("timeToRoll");
        cameraEventProp = serializedObject.FindProperty("cameraChange");
        actionTriggersProp = serializedObject.FindProperty("actionTriggers");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(entryCurveStrengthProp);
        EditorGUILayout.PropertyField(exitCurveStrengthProp);
        EditorGUILayout.PropertyField(modifiesSpeedProp);
        if (modifiesSpeedProp.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(newSpeedProp);
            EditorGUILayout.PropertyField(speedTransitionTimeProp);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.PropertyField(modifiesAngleProp);
        if (modifiesAngleProp.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(newAngleProp);
            EditorGUILayout.PropertyField(angleTransitionTimeProp);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.PropertyField(cameraEventProp);
        EditorGUILayout.PropertyField(actionTriggersProp);

        serializedObject.ApplyModifiedProperties();
    }
}
