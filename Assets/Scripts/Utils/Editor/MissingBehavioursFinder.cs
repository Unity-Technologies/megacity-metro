using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public static class MissingBehavioursFinder
{
    [MenuItem("Tools/Find missing behaviours/In prefabs")]
    public static void FindMissingBehavioursInPrefabs()
    {
        string[] paths = AssetDatabase.GetAllAssetPaths();

        foreach (var path in paths)
        {
            if (path.EndsWith(".prefab") && path.StartsWith("Assets"))
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                FindMissingBehaviours(prefab, path);
            }
        }
    }
    
    [MenuItem("Tools/Find missing behaviours/In scene")]
    public static void FindMissingBehavioursInScene()
    {
        for (int i = 0; i < EditorSceneManager.loadedRootSceneCount; i++)
        {
            Scene scene = EditorSceneManager.GetSceneAt(i);
            foreach (var go in scene.GetRootGameObjects())
            {
                FindMissingBehaviours(go, scene.name);
            }
        }
    }

    private static void FindMissingBehaviours(GameObject rootObject, string path)
    {
        foreach (var comp in rootObject.GetComponentsInChildren<Component>(true))
        {
            if (comp == null)
            {
                rootObject.hideFlags = HideFlags.None;
                Debug.LogError($"Found missing behaviour: {path} --- {rootObject.name}", rootObject);
            }
        }
    }
}
