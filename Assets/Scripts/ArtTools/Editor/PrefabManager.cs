using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class PrefabManager : EditorWindow
{
    [MenuItem("Tools/MegacityMetro/PrefabManager")]
    public static void ShowManager()
    {
        var p = GetWindow<PrefabManager>();
        p.MapLayout =
            AssetDatabase.LoadAssetAtPath<global::MapLayout>("Assets/Art/Models/Placeholder/WFC/MapLayout.asset");
        DefaultAsset v = AssetDatabase.LoadAssetAtPath<DefaultAsset>("Assets/Art/Models/Placeholder/WFC/");
        p.Show();
    }

    public MapLayout MapLayout;
    public DefaultAsset PrefabAssetFolder;
    public DefaultAsset OutputFolder;

    void OnGUI()
    {
        MapLayout = (MapLayout) EditorGUILayout.ObjectField("Map Layout", MapLayout, typeof(MapLayout), false);
        PrefabAssetFolder = (DefaultAsset) EditorGUILayout.ObjectField("Block Prefabs Folder", PrefabAssetFolder,
            typeof(DefaultAsset), false);
        OutputFolder =
            (DefaultAsset) EditorGUILayout.ObjectField("Layout Output Folder", OutputFolder, typeof(DefaultAsset),
                true);

        if (GUILayout.Button("Generate Prefabs"))
        {
            GenerateMapPrefabs();
        }

        if (GUILayout.Button("Create Layout"))
        {
            GenerateLayout();
        }
    }

    public void GenerateMapPrefabs()
    {
        if (PrefabAssetFolder == null)
        {
            Debug.LogError("Must select a folder");
            return;
        }

        var prefabPath = AssetDatabase.GetAssetPath(PrefabAssetFolder);
        var folderPath = AssetDatabase.GetAssetPath(OutputFolder);
        var folderPrefabs = AssetDatabase.FindAssets("Block_", new string[] {prefabPath});

        List<GameObject> instances = new List<GameObject>();

        foreach (var prefab in folderPrefabs)
        {
            var assetName = AssetDatabase.GUIDToAssetPath(prefab);
            instances.Add(AssetDatabase.LoadAssetAtPath<GameObject>(assetName));
        }

        var knockouts = new HashSet<int2>(MapLayout.Knockouts);
        int counter = 0;

        // generate the prefabs in the folder
        for (int x = 0; x < MapLayout.Blocks.x; x++)
        for (int y = 0; y < MapLayout.Blocks.y; y++)
        {
            if (knockouts.Contains(new int2(x, y)))
            {
                Debug.Log($"knockout {x},{y}");
                continue;
            }

            if (AssetDatabase.AssetPathToGUID($"{folderPath}/Block_{x}_{y}_prefab.prefab",
                    AssetPathToGUIDOptions.OnlyExistingAssets) != "")
            {
                AssetDatabase.DeleteAsset($"{folderPath}/Block_{x}_{y}_prefab.prefab");
                AssetDatabase.Refresh();
            }

            var blockRoot = new GameObject(name = $"Block_{x}_{y}");
            var rotator = new GameObject("rotator");
            rotator.transform.SetParent(blockRoot.transform);
            var r = Random.Range(0, 5) * 90f * Mathf.Deg2Rad;
            var placeholder = (GameObject) PrefabUtility.InstantiatePrefab(instances[counter], rotator.transform);
            placeholder.transform.localPosition = new Vector3(-200, 0, -200);

            rotator.transform.localRotation = quaternion.EulerXYZ(0, r, 0);
            PrefabUtility.SaveAsPrefabAsset(blockRoot, $"{folderPath}/{blockRoot.name}_prefab.prefab");
            DestroyImmediate(blockRoot);
            counter++;
            counter %= instances.Count;
        }
    }

    void GenerateLayout()
    {
        if (PrefabAssetFolder == null)
        {
            Debug.LogError("Must select a folder");
            return;
        }

        float offsetX = (MapLayout.Blocks.x - 1) / 2f;
        float offsetY = (MapLayout.Blocks.y - 1) / 2f;
        var folderPath = AssetDatabase.GetAssetPath(OutputFolder);

        Debug.Log($"Folderpath: {folderPath}");
        foreach (var eachAsset in AssetDatabase.FindAssets("prefab", new[] {folderPath}))
        {
            var assetName = AssetDatabase.GUIDToAssetPath(eachAsset);
            var fileName = System.IO.Path.GetFileName(assetName);
            char xx = fileName[6];
            char yy = fileName[8];
            int.TryParse(xx.ToString(), out int x);
            int.TryParse(yy.ToString(), out int y);
            var xPos = (x - offsetX) * (MapLayout.BlockSize.x + MapLayout.StreetSize / 2);
            var zPos = (y - offsetY) * (MapLayout.BlockSize.y + MapLayout.StreetSize / 2);

            var pf = AssetDatabase.LoadAssetAtPath<GameObject>(assetName);
            var bk = (GameObject) PrefabUtility.InstantiatePrefab(pf);
            bk.transform.position = new Vector3(xPos, 0, zPos);
        }
    }
}