using Unity.Mathematics;
using Unity.MegacityMetro.Traffic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace Unity.MegacityMetro.EditorTools
{
    /// <summary>
    /// Allows authoring the road data
    /// </summary>
    [CustomEditor(typeof(RoadSettings))]
    [CanEditMultipleObjects]
    public class RoadSettingsInspector : UnityEditor.Editor
    {
        private RoadSettings editingSettings;

        public void OnEnable()
        {
            editingSettings = (RoadSettings) target;
        }


        void AddNewPath()
        {
            UnityEngine.Camera cam = UnityEngine.Camera.current;
            if (cam == null)
            {
                cam = SceneView.lastActiveSceneView.camera;
            }

            RoadSettings me = target as RoadSettings;

            Vector3 spawnPos = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 32f));

            GameObject go = new GameObject("Road");
            go.transform.position = spawnPos;
            go.transform.rotation = cam.transform.rotation;
            go.transform.parent= me.transform;

            go.AddComponent<Path>();

            Selection.activeGameObject = go;

            Undo.RegisterCreatedObjectUndo(go,"Create New Traffic Path");
        }

        public override void OnInspectorGUI()
        {
            TrafficConfigAuthoring ts = editingSettings.GetComponentInParent<TrafficConfigAuthoring>();

            GUILayout.BeginVertical();
            EditorGUILayout.LabelField("Vehicles Allowed On Child Roads:");

            if (ts == null)
            {
                EditorGUILayout.LabelField("No Traffic Settings found In Parent Hierarchy!");
            }
            else
            {
                if (ts.vehiclePrefabs.Count == 0)
                {
                    EditorGUILayout.LabelField("Parent Traffic Settings should contain one of each vehicle in its vehicle pool!");
                }
                else
                {
                    uint output = 0;
                    bool[] toggles = new bool[ts.vehiclePrefabs.Count];
                    for (int a = 0; a < ts.vehiclePrefabs.Count; a++)
                    {
                        bool inputVal = (editingSettings.vehicleSelection & (1 << a)) != 0;
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(20.0f);
                        bool outputVal = EditorGUILayout.ToggleLeft("" + ts.vehiclePrefabs[a].name, inputVal);
                        GUILayout.EndHorizontal();

                        output |= (outputVal==false) ? 0 : (uint)(1 << a);
                    }

                    editingSettings.vehicleSelection = output;
                }
            }
            if (GUILayout.Button("Add A New Road"))
            {
                AddNewPath();
            }

            GUILayout.EndVertical();
        }

    }

    /// <summary>
    /// Author the path component and path nodes
    /// </summary>
    [CustomEditor(typeof(Path))]
    [CanEditMultipleObjects]
    public class EditorPath : UnityEditor.Editor
    {
        private Path editingPath;
        static bool showHandles;
        static float3 worldPosCopy=float3.zero;

        private ReorderableList nodesList;

        public void OnEnable()
        {
            editingPath = (Path) target;

            nodesList = new ReorderableList(serializedObject, serializedObject.FindProperty("Waypoints"), true, true,
                true, true);

            nodesList.drawHeaderCallback = (Rect rect) => { EditorGUI.LabelField(rect, "Road Waypoints"); };

            nodesList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = nodesList.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;

                Vector2 numberDimension = GUI.skin.button.CalcSize(new GUIContent("999"));
                Vector2 labelDimension = GUI.skin.label.CalcSize(new GUIContent("XX"));

                float width = rect.width;
                width -= numberDimension.x*3;
                width -= labelDimension.x * 4;
                width /= 4;

                if (GUI.Button(new Rect(rect.position, numberDimension),
                    new GUIContent(index.ToString(), "Go to the waypoint in the scene view")))
                {
                    if (SceneView.lastActiveSceneView != null)
                    {
                        nodesList.index = index;
                        SceneView.lastActiveSceneView.pivot = editingPath.GetWorldPosition(index);
                        SceneView.lastActiveSceneView.size = 30*3;
                        SceneView.lastActiveSceneView.Repaint();
                    }
                }

                float currentX = rect.x + numberDimension.x;

#if false
                EditorGUI.MultiPropertyField(
                    new Rect(currentX, rect.y, (labelDimension.x + width) * 3, EditorGUIUtility.singleLineHeight),
                    new GUIContent[] {new GUIContent("X"),new GUIContent("Y"),new GUIContent("Z")  },
                    element.FindPropertyRelative("position"));
                currentX+=(labelDimension.x+width)*3;

#else
                var labelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 25f;
                EditorGUI.PropertyField(new Rect(currentX, rect.y, width, EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("position.x"));
                currentX += labelDimension.x+width;
                EditorGUI.PropertyField(new Rect(currentX, rect.y, width, EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("position.y")/*, new GUIContent("Y")*/);
                currentX += labelDimension.x+width;
                EditorGUI.PropertyField(new Rect(currentX, rect.y, width, EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("position.z")/*, new GUIContent("Z")*/);
                currentX += labelDimension.x+width;
                EditorGUIUtility.labelWidth = labelWidth;
#endif
                if (index < editingPath.GetNumNodes() - 1)
                {
                    float arcLength = math.round(editingPath.ComputeArcLength(index, 256));
                    Color prev = GUI.contentColor;
                    if (arcLength / Constants.VehicleLength > Constants.RoadOccupationSlotsMax)
                        GUI.contentColor = Color.blue;
                    EditorGUI.LabelField(
                        new Rect(currentX, rect.y, width+labelDimension.x, EditorGUIUtility.singleLineHeight),
                        "Arc Len = " + arcLength);
                    GUI.contentColor = prev;
                    currentX += width+labelDimension.x;
                }

                if (GUI.Button(new Rect(currentX, rect.y, numberDimension.x,numberDimension.y),
                    new GUIContent("C", "Copy absolute node position")))
                {
                    worldPosCopy = editingPath.GetRawPosition(index) + new float3(editingPath.transform.position);
                }
                currentX += numberDimension.x;

                if (GUI.Button(new Rect(currentX, rect.y, numberDimension.x,numberDimension.y),
                    new GUIContent("P", "Paste absolute node position")))
                {
                    Undo.RecordObject(editingPath, "Paste world position");
                    editingPath.SetRawPosition(index,worldPosCopy - new float3(editingPath.transform.position));
                }

            };

            nodesList.onCanRemoveCallback = (ReorderableList l) => { return l.count > 2; };

            nodesList.onAddCallback = (ReorderableList l) =>
            {
                Undo.RecordObject(editingPath, "Adding a new road node");
                editingPath.AddNewNode(l.index);
                if (SceneView.lastActiveSceneView != null)
                {
                    SceneView.lastActiveSceneView.Repaint();
                }
            };
        }

        public override void OnInspectorGUI()
        {
            if (targets.Length == 1)
            {
                serializedObject.Update();
                nodesList.DoLayoutList();
                serializedObject.ApplyModifiedProperties();
            }

            base.OnInspectorGUI();
            GUILayout.BeginVertical();
            bool nShowHandles = GUILayout.Toggle(showHandles, "Show All Handles");
            if (nShowHandles != showHandles)
            {
                showHandles = nShowHandles;
                if (SceneView.lastActiveSceneView != null)
                {
                    SceneView.lastActiveSceneView.Repaint();
                }
            }
            bool nShowColoured = GUILayout.Toggle(Path.GetShowColoured(), "Show Coloured Roads");
            if (nShowColoured != Path.GetShowColoured())
            {
                Path.SetShowColoured(nShowColoured);
                if (SceneView.lastActiveSceneView != null)
                {
                    SceneView.lastActiveSceneView.Repaint();
                }
            }
            GUILayout.EndVertical();
        }

        void HandleMove()
        {
            if (!showHandles)
            {
                return;
            }

            if (Selection.activeGameObject == editingPath.gameObject)
            {
                // Draw handles for all the objects ...
                for (int a = 0; a < editingPath.GetNumNodes(); a++)
                {
                    bool changed = false;
                    float3 rawPos = editingPath.GetRawPosition(a) + new float3(editingPath.transform.position);
                    float3 ePos = rawPos - editingPath.height * new float3(0, 0.5f, 0);

                    if (Handles.Button(ePos,editingPath.transform.rotation, 4, 4, Handles.SphereHandleCap))
                    {
                        nodesList.index = a;
                        InternalEditorUtility.RepaintAllViews();
                    }

                    // Label it
                    Handles.BeginGUI();
                    Vector2 labelSize = new Vector2(
                        EditorGUIUtility.singleLineHeight * 2, EditorGUIUtility.singleLineHeight);
                    Vector2 labelPos = HandleUtility.WorldToGUIPoint(ePos);
                    labelPos.y -= labelSize.y / 2;
                    labelPos.x -= labelSize.x / 2;
                    GUILayout.BeginArea(new Rect(labelPos, labelSize));
                    GUIStyle style = new GUIStyle();
                    style.normal.textColor = Color.black;
                    style.alignment = TextAnchor.MiddleCenter;
                    GUILayout.Label(new GUIContent(a.ToString(), "Waypoint " + a), style);
                    GUILayout.EndArea();
                    Handles.EndGUI();

                    EditorGUI.BeginChangeCheck();
                    Vector3 targetPos = Handles.PositionHandle(rawPos, editingPath.transform.rotation);

                    if (EditorGUI.EndChangeCheck())
                    {
                        rawPos = targetPos;
                        changed = true;
                    }

                    if (changed)
                    {
                        Undo.RecordObject(editingPath, "Moving Node/Handle on Path");
                        editingPath.SetRawPosition(a, rawPos - new float3(editingPath.transform.position));
                    }
                }
            }

        }

        void OnSceneGUI()
        {
            if (Tools.current != Tool.Move)
            {

            }
            else
            {
                HandleMove();
            }


        }
    }
}
