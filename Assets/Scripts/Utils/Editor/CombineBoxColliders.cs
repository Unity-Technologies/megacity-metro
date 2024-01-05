using System.IO;
using UnityEditor;
using UnityEngine;

public class CombineBoxColliders : EditorWindow
{
    private Mesh m_Mesh;
    
    [MenuItem("Tools/Combine Box Colliders")]
    public static void ShowWindow()
    {
        GetWindow(typeof(CombineBoxColliders));
    }

    private void OnGUI()
    {
        GUILayout.Label("Combine Box Colliders", EditorStyles.boldLabel);
        m_Mesh = (Mesh)EditorGUILayout.ObjectField("Cube", m_Mesh, typeof(Mesh), false);
        var meshFilters = FindObjectsByType<BoxCollider>(FindObjectsSortMode.None);
        if (GUILayout.Button("Combine Box Colliders") && meshFilters.Length > 0)
        {
            var sceneName = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().name;
            sceneName = string.IsNullOrEmpty(sceneName) ? "MeshColliderFromScene" : sceneName; 
            var combine = new CombineInstance[meshFilters.Length];
            int i = 0;
            while (i < meshFilters.Length)
            {
                var boxCollider = meshFilters[i];
                var scale = boxCollider.transform.lossyScale;
                
                var meshScale = new Vector3(boxCollider.size.x * scale.x,
                    boxCollider.size.y * scale.y,
                    boxCollider.size.z * scale.z);

                var meshPosition = new Vector3(boxCollider.center.x * scale.x,
                    boxCollider.center.y * scale.y,
                    boxCollider.center.z * scale.z);

                var boxColliderMatrix = Matrix4x4.TRS(meshPosition, boxCollider.transform.rotation, meshScale);
                
                combine[i].mesh = GenerateMesh();
                combine[i].transform = Matrix4x4.TRS(meshPosition + boxCollider.transform.position, meshFilters[i].transform.rotation, meshScale);// meshFilters[i].transform.localToWorldMatrix * boxColliderMatrix;
                    
                
                i++;
            }

            var mesh = new Mesh();
            mesh.CombineMeshes(combine);

            var path = "Assets/Art/Models/SceneCollider";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
           
            AssetDatabase.CreateAsset(mesh, $"{path}/{sceneName}.mesh");
            AssetDatabase.SaveAssets();
        }
        
        
    }
    
    private Mesh GenerateMesh()
    {
        var mesh = new Mesh();
        mesh.name = "BoxColliderMesh";
        
        // Assign the vertices and triangles to the mesh
        mesh.vertices = m_Mesh.vertices;
        mesh.triangles = m_Mesh.triangles;
        // Recalculate the normals to ensure proper lighting
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        
        // Set the mesh to the MeshFilter component
        return mesh;
    }
}
