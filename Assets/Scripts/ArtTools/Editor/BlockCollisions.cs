using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class BlockCollisions : EditorWindow
{
    public static HashSet<Vector3Int> Voxelize(GameObject prefab, Bounds b, int gridSize = 16)
    {
        float verticalQuantize = 3f;
        var x = Mathf.CeilToInt(b.size.x / gridSize);
        var z = Mathf.CeilToInt(b.size.z / gridSize);
        var hits = new HashSet<Vector3Int>();

        for (int xx = 0; xx < x; xx++)
        for (int zz = 0; zz < z; zz++)
        {
            var source = new Vector3((xx + 0.5f) * gridSize, b.size.y + (gridSize / 2f), (zz + 0.5f) * gridSize);

            var dir = new Vector3(0, -1, 0);
            var Ray = new Ray(source, dir);
            var didHit = Physics.SphereCast(Ray, gridSize / 4f, out var hitInfo, 1000);
            if (didHit)
            {
                var quantizedY =
                    Mathf.FloorToInt(Mathf.RoundToInt(hitInfo.point.y / verticalQuantize) * verticalQuantize);
                Debug.Log($"{xx}, {zz} ={quantizedY}");

                var center = new Vector3((xx + 0.5f) * gridSize, gridSize, (zz + 0.5f) * gridSize);
                if (b.Contains(center)) hits.Add(new Vector3Int(xx, quantizedY, zz));
            }
        }

        return hits;
    }

    [MenuItem("Tools/MegacityMetro/Collision/32m")]
    public static void Collision32()
    {
        int sample = 32;
        var sel = Selection.activeGameObject;

        using (new TemporaryMeshColliders(sel))
        {
            GeneratedCollisionGrid(sel, sample);
        }
    }

    [MenuItem("Tools/MegacityMetro/Collision/16m")]
    public static void Collision16()
    {
        int sample = 16;
        var sel = Selection.activeGameObject;

        using (new TemporaryMeshColliders(sel))
        {
            GeneratedCollisionGrid(sel, sample);
        }
    }

    [MenuItem("Tools/MegacityMetro/Collision/64m")]
    public static void Collision48()
    {
        int sample = 64;
        var sel = Selection.activeGameObject;

        using (new TemporaryMeshColliders(sel))
        {
            GeneratedCollisionGrid(sel, sample);
        }
    }

    static void GeneratedCollisionGrid(GameObject sel, int sample)
    {
        var b = CollectBounds(sel);

        b.min += new Vector3(sample / 4f, 0, sample / 4f);
        b.max -= new Vector3(sample / 4f, 0, sample / 4f);

        var h = Voxelize(sel, b, sample);

        GameObject center = new GameObject("Collision");
        center.transform.position = new Vector3(200, 0, 200);

        HashSet<Vector3Int> dead = new HashSet<Vector3Int>();

        foreach (var eachHit in h)
        {
            if (dead.Contains(eachHit)) continue;
            var right = new Vector3Int(eachHit.x + 1, eachHit.y, eachHit.z);
            var right2 = new Vector3Int(eachHit.x + 2, eachHit.y, eachHit.z);
            var up = new Vector3Int(eachHit.x, eachHit.y, eachHit.z + 1);
            var up2 = new Vector3Int(eachHit.x, eachHit.y, eachHit.z + 2);
            var upright = new Vector3Int(eachHit.x + 1, eachHit.y, eachHit.z + 1);
            var upright2 = new Vector3Int(eachHit.x + 2, eachHit.y, eachHit.z + 2);

            var hasRight = h.Contains(right);
            var hasRight2 = h.Contains(right2);
            var hasUp = h.Contains(up);
            var hasUp2 = h.Contains(up2);
            var isSquare = hasRight & hasUp & h.Contains(upright);

            if (isSquare)
            {
                dead.Add(eachHit);
                dead.Add(right);
                dead.Add(up);
                dead.Add(upright);
                var g = new GameObject($"Square {eachHit.x},{eachHit.y},{eachHit.z}", typeof(BoxCollider));
                g.GetComponent<BoxCollider>().size = new Vector3(sample * 2, eachHit.y, sample * 2);

                g.transform.position =
                    new Vector3((eachHit.x + 1f) * sample, (eachHit.y / 2f), (eachHit.z + 1f) * sample);
                g.transform.parent = center.transform;
                continue;
            }

            if (hasRight && hasRight2)
            {
                dead.Add(eachHit);
                dead.Add(right);
                dead.Add(right2);

                var g = new GameObject($"Right2 {eachHit.x},{eachHit.y},{eachHit.z}", typeof(BoxCollider));
                g.GetComponent<BoxCollider>().size = new Vector3(sample * 3, eachHit.y, sample);
                g.transform.position = new Vector3((eachHit.x + 1.5f) * sample, (eachHit.y / 2f),
                    (eachHit.z + 0.5f) * sample);
                g.transform.parent = center.transform;
                continue;
            }

            if (hasUp && hasUp2)
            {
                dead.Add(eachHit);
                dead.Add(up);
                dead.Add(up2);
                var g = new GameObject($"Up2 {eachHit.x},{eachHit.y},{eachHit.z}", typeof(BoxCollider));
                g.GetComponent<BoxCollider>().size = new Vector3(sample, eachHit.y, sample * 3);
                g.transform.position = new Vector3((eachHit.x + 0.5f) * sample, (eachHit.y / 2f),
                    (eachHit.z + 1.5f) * sample);
                g.transform.parent = center.transform;
                continue;
            }

            if (hasRight)
            {
                dead.Add(eachHit);
                dead.Add(right);
                var g = new GameObject($"Right {eachHit.x},{eachHit.y},{eachHit.z}", typeof(BoxCollider));
                g.GetComponent<BoxCollider>().size = new Vector3(sample * 2, eachHit.y, sample);
                g.transform.position =
                    new Vector3((eachHit.x + 1f) * sample, (eachHit.y / 2f), (eachHit.z + 0.5f) * sample);
                g.transform.parent = center.transform;
                continue;
            }

            if (hasUp)
            {
                dead.Add(eachHit);
                dead.Add(up);
                var g = new GameObject($"up {eachHit.x},{eachHit.y},{eachHit.z}", typeof(BoxCollider));
                g.GetComponent<BoxCollider>().size = new Vector3(sample, eachHit.y, sample * 2);
                g.transform.position =
                    new Vector3((eachHit.x + 0.5f) * sample, (eachHit.y / 2f), (eachHit.z + 1f) * sample);
                g.transform.parent = center.transform;
            }
            else
            {
                dead.Add(eachHit);
                var g = new GameObject($"{eachHit.x},{eachHit.y},{eachHit.z}", typeof(BoxCollider));
                g.GetComponent<BoxCollider>().size = new Vector3(sample, eachHit.y, sample);
                g.transform.position = new Vector3((eachHit.x + 0.5f) * sample, (eachHit.y / 2f),
                    (eachHit.z + 0.5f) * sample);
                g.transform.parent = center.transform;
            }
        }
    }

    static Bounds CollectBounds(GameObject sel)
    {
        var b = new Bounds();
        foreach (var eachMesh in sel.GetComponentsInChildren<MeshRenderer>())
        {
            b.Encapsulate(eachMesh.bounds);
        }

        return b;
    }

    [MenuItem("MegacityMetro/Attachments")]
    static void AttachTest()
    {
        var placeables = AssetDatabase.FindAssets("place", new[] {"Assets/Prefabs/Environment/Signage/Placeable"});
        var sel = Selection.activeGameObject;
        var points = GenerateAttachments(sel);

        void InstancePrefab(GameObject root)
        {
            var r = Random.Range(0, placeables.Length);
            var guid = placeables[r];
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var prototype = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var p = (GameObject) PrefabUtility.InstantiatePrefab(prototype, root.transform);
            p.transform.localPosition = Vector3.zero;
            p.transform.localRotation = Quaternion.identity;
        }

        foreach (var point in points)
        {
            InstancePrefab(point);
            point.transform.SetParent(sel.transform, true);
        }
    }

    static IEnumerable<GameObject> GenerateAttachments(GameObject sel, float minHeight = 30f,
        float maxHeight = 130f,
        int minSamplesPerSide = 6, int maxSamplesPerSide = 12)
    {
        var b = CollectBounds(sel);
        var center = b.center;
        center.y = 0;
        var dist = Mathf.Max(b.size.x, b.size.z);

        Vector3[] directions = {Vector3.left, Vector3.right, Vector3.forward, Vector3.back,};
        float[] widths = {b.size.z, b.size.z, b.size.x, b.size.x};
        float[] starts = {b.min.z, b.min.z, b.min.x, b.min.x};

        using (new TemporaryMeshColliders(sel))
        {
            for (int side = 0; side < 4; side++)
            {
                int samplesThisSide = Random.Range(minSamplesPerSide, maxSamplesPerSide);
                var dir = directions[side];
                var start = starts[side];
                var w = widths[side];
                var cellSize = w / (samplesThisSide);
                for (int s = 0; s < samplesThisSide; s++)
                {
                    var cellNum = s - (samplesThisSide / 2);
                    var cellOffset = 0.5f + Random.Range(-0.33f, 0.33f);
                    var cellPos = start + (0.9f * ((cellNum + cellOffset) * cellSize));
                    var hpct = Random.Range(0f, 1f);
                    hpct = Mathf.Pow(hpct, 2.5f);

                    var heightOffset = Mathf.Lerp(minHeight, maxHeight, hpct);

                    var source = Vector3.Cross(dir, Vector3.up) * cellPos * -1;
                    source += Vector3.up * heightOffset;
                    source += dir * -1 * dist;
                    source += center;

                    var r = new Ray(source, dir);
                    var h = Physics.SphereCast(r, 10, out var raycastHit, dist / 2f + 50);
                    if (h)
                    {
                        var flatProj = new Vector3(raycastHit.normal.x, 0, raycastHit.normal.z);
                        flatProj.Normalize();
                        var ax = Mathf.Abs(flatProj.x);
                        var ay = Mathf.Abs(flatProj.z);
                        if (ax + ay > 1.01 && (!Mathf.Approximately(ax, ay)))
                        {
                            continue;
                        }

                        var c = new GameObject();
                        c.name = $"attach_{side}_{s}";
                        c.transform.position = raycastHit.point;

                        c.transform.rotation = Quaternion.LookRotation(flatProj);
                        yield return c;
                    }
                }
            }
        }
    }
}

public struct AttachmentPoint
{
    public Vector3 Position;
    public Vector3 Normal;
}

public class TemporaryMeshColliders : IDisposable
{
    HashSet<MeshCollider> Colliders = new();

    public TemporaryMeshColliders(GameObject root)
    {
        Debug.LogFormat("adding temporary mesh colliders");
        HashSet<GameObject> objects = new HashSet<GameObject>();
        foreach (var meshRenderer in root.GetComponentsInChildren<MeshRenderer>())
        {
            var target = meshRenderer.gameObject;
            if (!target.activeInHierarchy)
            {
                Debug.Log($"skip inactive game object {target}");
            }
            if (target.GetComponent<MeshCollider>())
            {
                Debug.Log($"ignore existing colliders on {target}");
            }

            objects.Add(meshRenderer.gameObject);
        }

        foreach (var go in objects)
        {
            var newCollider = go.AddComponent<MeshCollider>();
            Colliders.Add(newCollider);
        }
    }

    public void Dispose()
    {
        Debug.Log("Removing temporary MeshColliders");
        foreach (var meshCollider in Colliders)
        {
            UnityEngine.Object.DestroyImmediate(meshCollider);
        }
    }
}