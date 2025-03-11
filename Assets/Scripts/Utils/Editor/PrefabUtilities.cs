using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.MegacityMetro.EditorTools
{
    public static class PrefabUtilities
    {
        /// <summary>
        /// Helper function to allow reverting prefab changes from selection
        /// </summary>
        [MenuItem("Tools/Prefabs/Revert Selection")]
        static void RevertSelection()
        {
            var gameObjectSelection = Selection.gameObjects;
            Undo.RegisterCompleteObjectUndo(gameObjectSelection, "revert selection");
            foreach (var go in gameObjectSelection)
            {
                if (go != null && PrefabUtility.IsPartOfNonAssetPrefabInstance(go) &&
                    PrefabUtility.IsOutermostPrefabInstanceRoot(go))
                {
                    PrefabUtility.RevertPrefabInstance(go, InteractionMode.AutomatedAction);
                }
            }
        }

        [MenuItem("Tools/Prefabs/Revert Selection (preserve scale)")]
        static void RevertSelectionPreserveScale()
        {
            var gameObjectSelection = Selection.gameObjects;
            Undo.RegisterCompleteObjectUndo(gameObjectSelection, "revert selection");
            foreach (var go in gameObjectSelection)
            {
                if (go != null && PrefabUtility.IsPartOfNonAssetPrefabInstance(go) &&
                    PrefabUtility.IsOutermostPrefabInstanceRoot(go))
                {
                    var t = EditorJsonUtility.ToJson(go.transform);
                    PrefabUtility.RevertPrefabInstance(go, InteractionMode.AutomatedAction);
                    EditorJsonUtility.FromJsonOverwrite(t, go.transform);
                }
            }
        }


        [MenuItem("Tools/Prefabs/Find Unused Buildings")]
        static void FindUnused()
        {
            var cityPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Blocks/CityLayout.prefab");
            var all_meshes = cityPrefab.GetComponentsInChildren<MeshFilter>();

            HashSet<string> used_fbxes = new HashSet<string>();
            foreach (var eachMesh in all_meshes)
            {
                var s = AssetDatabase.GetAssetPath(eachMesh.sharedMesh);
                if (s.Contains("ProcBuildings")) used_fbxes.Add(s);
            }

            var all_building_models = AssetDatabase.FindAssets("t:model", new [] {"Assets/Art/Models/Environment/ProcBuildings"});
            HashSet<string> all_model_fbxes = new HashSet<string>();
            foreach (var eachbldg in all_building_models)
            {
                all_model_fbxes.Add(AssetDatabase.GUIDToAssetPath(eachbldg));
            }

            var unused = all_model_fbxes.Except(used_fbxes);
            Debug.Log("used:\n" + string.Join('\n', used_fbxes));
            Debug.Log("unused\n" + string.Join('\n', unused));

            Debug.Log($"Deleting {unused.Count()} models");
            List<string> failed = new List<string>();
            AssetDatabase.DeleteAssets(unused.ToArray(), failed);
            foreach (var bad_path in failed)
            {
                Debug.Log($"failed to delete {bad_path}");
            }

        }
            
        
    }
}
