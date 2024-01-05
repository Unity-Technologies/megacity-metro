using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

public class MaterialLODTool : MonoBehaviour
{
    // I didn't have time to figure out the correct incantation
    // to edit the prefabs directly on disk -- so you have to
    // 1. make an empty scene
    // 2. drag all the prefabs to edit in
    // 3. Select all
    // 4. Choose this menu item
    // 5. Save all the overrides
    // not sure why this is so messy otherwise...  - ST

    [MenuItem("Tools/MegacityMetro/Assign LOD Shaders")]
    public static void UpdatePrefabMaterials()
    {
        var replacementList =
            AssetDatabase.LoadAssetAtPath<ReplacementList>(
                "Assets/Art/Models/Environment/ProcBuildings/ShaderReplace.asset");

        Dictionary<Material, Material[]> replacementDict = new Dictionary<Material, Material[]>();

        foreach (var replacementSet in replacementList.Replacement)
        {
            replacementDict[replacementSet.Original] = new Material[]
                {replacementSet.Original, replacementSet.LOD1, replacementSet.LOD2, replacementSet.LOD3};
        }

        Debug.Log($"{replacementDict.Count} replacement sets found");


        foreach (var go in Selection.gameObjects)
        {
            foreach (var mr in go.GetComponentsInChildren<MeshRenderer>())
            {
                Debug.Log($"checking ${mr.gameObject}");
                var mrgo = mr.gameObject.name.ToLower();

                var lod = Int32.Parse(mrgo[mrgo.IndexOf("lod") + 3].ToString());

                List<Material> mats = new List<Material>();
                foreach (var eachMat in mr.sharedMaterials)
                {
                    if (replacementDict.ContainsKey(eachMat) & lod > 0)
                    {
                        var newMat = replacementDict[eachMat][lod];
                        mats.Add(newMat);
                    }
                    else
                    {
                        mats.Add(eachMat);
                    }
                }

                StringBuilder sb = new StringBuilder();
                foreach (var material in mats)
                {
                    sb.Append(material.name);
                    sb.Append("-");
                }

                Debug.Log(sb.ToString());
                mr.SetSharedMaterials(mats);
            }
        }
    }

    [Serializable]
    public struct ReplacementSet
    {
        public Material Original;
        public Material LOD1;
        public Material LOD2;
        public Material LOD3;
    }

    [CreateAssetMenu(fileName = "ShaderReplace", menuName = "MegacityMetro/Shader LOD Set", order = 4)]
    public class ReplacementList : ScriptableObject
    {
        public ReplacementSet[] Replacement;
    }
}