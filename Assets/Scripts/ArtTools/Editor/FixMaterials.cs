using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FixMaterials : EditorWindow
{
    public MaterialMap MaterialMap;

    void OnGUI()
    {
        MaterialMap =
            (MaterialMap) EditorGUILayout.ObjectField("Material Map", MaterialMap, typeof(MaterialMap), false);

        if (GUILayout.Button("Assign"))
        {
            Assign();
        }
    }

    [MenuItem("Tools/MegacityMetro/Update Materials")]
    public static void ShowDialog()
    {
        CreateInstance<FixMaterials>().ShowUtility();
        ;
    }

    void Assign()
    {
        foreach (var eachGuid in Selection.assetGUIDs)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(eachGuid);
            //var go = AssetDatabase.LoadAssetAtPath<GameObject>(apath);//
            var pu = PrefabUtility.LoadPrefabContents(assetPath);
            var mat = pu.GetComponentInChildren<MeshRenderer>();

            var c = mat.sharedMaterials.Length;
            switch (c)
            {
                case 1:
                {
                    break;
                }
                case 2:
                {
                    mat.SetSharedMaterials(new List<Material>()
                        {MaterialMap.Assignments[0].Material, MaterialMap.Assignments[1].Material});
                    break;
                }
                case 3:
                {
                    mat.SetSharedMaterials(new List<Material>()
                    {
                        MaterialMap.Assignments[0].Material, MaterialMap.Assignments[1].Material,
                        MaterialMap.Assignments[2].Material
                    });

                    Debug.Log("3");
                    break;
                }
            }

            EditorUtility.SetDirty(mat);
            EditorUtility.SetDirty(pu);

            PrefabUtility.SaveAsPrefabAsset(pu, assetPath);
            PrefabUtility.UnloadPrefabContents(pu);
        }
    }
}