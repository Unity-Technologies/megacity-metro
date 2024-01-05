using UnityEditor;
using UnityEngine;

namespace Unity.MegacityMetro.EditorTools
{
    public class InvertMeshNormals : EditorWindow
    {
        private Mesh mesh;

        [MenuItem("Tools/Invert Mesh Normals")]
        public static void ShowWindow()
        {
            GetWindow(typeof(InvertMeshNormals));
        }

        private void OnGUI()
        {
            GUILayout.Label("Invert Mesh Normals", EditorStyles.boldLabel);

            mesh = (Mesh)EditorGUILayout.ObjectField("Mesh", mesh, typeof(Mesh), false);

            if (GUILayout.Button("Invert Normals") && mesh != null)
            {
                var invertedMesh = Instantiate(mesh);
                var normals = invertedMesh.normals;
                for (int i = 0; i < normals.Length; i++)
                {
                    normals[i] = -normals[i];
                }
                invertedMesh.normals = normals;

                AssetDatabase.CreateAsset(invertedMesh, "Assets/NewInvertedMesh.mesh");
                AssetDatabase.SaveAssets();
            }
        }
    }
}