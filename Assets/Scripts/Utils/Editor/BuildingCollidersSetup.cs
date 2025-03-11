using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Unity.MegacityMetro.EditorTools
{
    public static class BuildingCollidersSetup
    {
        
        [MenuItem("Tools/MegacityMetro/Collision/Assign mesh colliders to buildings")]
        public static void AssignMeshColliders()
        {
            string[] paths = AssetDatabase.GetAllAssetPaths();

            foreach (var path in paths)
            {
                if (path.EndsWith(".prefab") && path.StartsWith("Assets/Prefabs/Blocks"))
                {
                    GameObject prefabRoot = PrefabUtility.LoadPrefabContents(path);

                    // Find collisions object
                    if (FindChildGameObjectWithName("Collision", prefabRoot, out GameObject collisionsObject))
                    {
                        if(!PrefabUtility.IsPartOfPrefabInstance(collisionsObject))
                        {
                            // Clear existing collisions
                            foreach (var coll in collisionsObject.GetComponentsInChildren<Collider>(true))
                            {
                                GameObject.DestroyImmediate(coll.gameObject, true);
                            }

                            collisionsObject.transform.parent = prefabRoot.transform;
                            collisionsObject.transform.position = default;
                            collisionsObject.transform.rotation = quaternion.identity;
                            collisionsObject.transform.localScale = Vector3.one;

                            // Find the mesh to use for collision
                            if (collisionsObject != null)
                            {
                                LODGroup[] lodGroups = prefabRoot.GetComponentsInChildren<LODGroup>(true);
                                foreach (LODGroup lodGroup in lodGroups)
                                {
                                    // Get the mesh of the simplest LOD
                                    LOD[] lods = lodGroup.GetLODs();
                                    if (lods.Length > 0)
                                    {
                                        LOD selectedLOD = lods[lods.Length - 1];

                                        // Select the first renderer
                                        if (selectedLOD.renderers.Length > 0)
                                        {
                                            Renderer selectedRenderer = selectedLOD.renderers[0];
                                            MeshFilter meshFilter = selectedRenderer.GetComponent<MeshFilter>();

                                            // Create a collisions copy under the Collisions object
                                            if (meshFilter != null)
                                            {
                                                GameObject collisionsCopy = new GameObject(meshFilter.gameObject.name + "_Collisions");
                                                collisionsCopy.transform.parent = collisionsObject.transform;

                                                MeshCollider meshCollider = collisionsCopy.GetComponent<MeshCollider>();
                                                if (meshCollider == null)
                                                {
                                                    meshCollider = collisionsCopy.AddComponent<MeshCollider>();
                                                }
                                                meshCollider.sharedMesh = meshFilter.sharedMesh;
                                                meshCollider.convex = false;

                                                collisionsCopy.transform.position = meshFilter.transform.position;
                                                collisionsCopy.transform.rotation = meshFilter.transform.rotation;
                                                collisionsCopy.transform.localScale = meshFilter.transform.lossyScale;
                                            }
                                        }
                                    }
                                }
                            }

                            // Save
                            PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);
                        }
                    }
                    
                    PrefabUtility.UnloadPrefabContents(prefabRoot);
                }
            }
            
            AssetDatabase.Refresh();
        }

        private static bool FindChildGameObjectWithName(string name, GameObject parent, out GameObject go)
        {
            foreach (Transform t in parent.GetComponentsInChildren<Transform>(true))
            {
                if (t.gameObject.name == name)
                {
                    go = t.gameObject;
                    return true;
                }
            }

            go = default;
            return false;
        }
    }
}