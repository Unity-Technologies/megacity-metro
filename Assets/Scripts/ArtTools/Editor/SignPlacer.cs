using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class SignPlacer : EditorWindow
{
    [Serializable]
    public struct SignItem
    {
        public GameObject Prefab;
        public float Percentage;

        public override string ToString()
        {
            var n = Prefab?.name ?? "test";
            return $"{n} : {Percentage}";
        }
    }

    public float BlockSize;
    public float StreetSize;
    public float Buffer;
    public SignInfo Info;
    SerializedObject Serial;

    void OnGUI()
    {
        BlockSize = EditorGUILayout.FloatField("Block Size", BlockSize);
        StreetSize = EditorGUILayout.FloatField("Street Size", StreetSize);
        Buffer = EditorGUILayout.FloatField("Buffer", Buffer);
        Info = (SignInfo) EditorGUILayout.ObjectField("Sign Info", Info, typeof(SignInfo), false);

        if (GUILayout.Button("Test"))
        {
            GenerateABlock();
        }
    }

    void GenerateABlock()
    {
        GameObject root = new GameObject("StreetSigns");

        var total = Random.Range(Info.SignsPerBlockRange.x, Info.SignsPerBlockRange.y);
        var array = new List<GameObject>();
        foreach (var eachItem in Info.Items)
        {
            for (int i = 0; i < eachItem.Percentage; i++)
            {
                array.Add(eachItem.Prefab);
            }
        }

        var seen = new HashSet<GameObject>();

        GameObject GetRandom()
        {
            var r = Random.Range(0, array.Count * 5) % array.Count;
            return array[r];
        }

        float cell = (BlockSize - (Buffer * 2)) / total;

        for (int j = 0; j <= total; j++)
        {
            GameObject proto = null;
            var found = false;
            while (!found)
            {
                var g = GetRandom();
                if (seen.Contains(g)) continue;
                seen.Add(g);
                proto = g;
                found = true;
            }

            var name = proto.name;

            var b = proto.GetComponentInChildren<MeshRenderer>()?.bounds ?? new Bounds(Vector3.zero, Vector3.one * 30);

            var side = (((j % 2) * 2) - 1);
            var offsetx = (cell * j + 0.5f);
            var offsetz = side * StreetSize / 2;
            var offsety = Random.Range(2f, 6f) * b.extents.y;
            var placement = (GameObject) PrefabUtility.InstantiatePrefab(proto, root.transform);
            placement.transform.position = new Vector3(offsetz, offsety, offsetx + Buffer);

            var rotation = Quaternion.identity;
            if (name.ToLower().Contains("rounded"))
            {
                if ((j % 2) == 0) rotation = quaternion.EulerXYZ(0, 180 * Mathf.Deg2Rad, 0);
            }

            if (name.ToLower().Contains("50x25") || name.ToLower().Contains("curved"))
            {
                if ((j % 2) == 0)
                {
                    rotation = quaternion.EulerXYZ(0, -90 * Mathf.Deg2Rad, 0);
                }
                else
                {
                    rotation = quaternion.EulerXYZ(0, 90 * Mathf.Deg2Rad, 0);
                }
            }

            if (name.ToLower().Contains("20x10"))
            {
                if (Random.Range(0, 6) > 4)
                {
                    rotation = quaternion.EulerXYZ(0, 90 * Mathf.Deg2Rad, 0);
                }
            }


            placement.transform.rotation = rotation;
        }
    }

    [MenuItem("Tools/MegacityMetro/Sign Placer")]
    public static void ShowMe()
    {
        GetWindow<SignPlacer>().Show();
    }
}