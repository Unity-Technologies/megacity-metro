using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace WaveFunctionCollapse
{
    public class BlockWFC : EditorWindow
    {
        Dictionary<int2, List<GameObject>> PrefabSet = new();

        public BlockDefinition BlockDefinition;
        public DefaultAsset PrefabFolder;
        public bool AllowIsolated;
        public float AirshaftChance = 0.5f;
        public int Manual = 999;
        public float FinalScale = 80;

        private void OnGUI()
        {
            BlockDefinition = (BlockDefinition) EditorGUILayout.ObjectField("Block Definitions", BlockDefinition,
                typeof(BlockDefinition), false);
            PrefabFolder =
                (DefaultAsset) EditorGUILayout.ObjectField("PrefabFolder", PrefabFolder, typeof(DefaultAsset), false);


            AllowIsolated = EditorGUILayout.Toggle("Allow Isolated", AllowIsolated);
            AirshaftChance = EditorGUILayout.FloatField("Airshaft chance", AirshaftChance);
            Manual = EditorGUILayout.IntField("Explicit Seed", Manual);
            FinalScale = EditorGUILayout.FloatField("Scale", FinalScale);

            if (GUILayout.Button("Test"))
            {
                int seed = Random.Range(1000, 9999);
                GenerateBlock(seed);
            }

            if (GUILayout.Button("Explicit Seed"))
            {
                GenerateBlock(Manual);
            }

            if (GUILayout.Button("Audit"))
            {
                StringBuilder Audit = new StringBuilder();

                var hgo = new Dictionary<GameObject, int>();
                foreach (var eachBlock in BlockDefinition.Blocks)
                {
                    hgo[eachBlock.PrefabName] = 0;
                }

                Audit.Append($"Using {hgo.Count} unique buildings\n");

                var bs = new Dictionary<string, int>();
                var block = AssetDatabase.FindAssets("Block", new[] {AssetDatabase.GetAssetPath(PrefabFolder)});
                foreach (var b in block)
                {
                    var t = PrefabUtility.LoadPrefabContents(AssetDatabase.GUIDToAssetPath(b));
                    foreach (var eachMR in t.GetComponentsInChildren<MeshRenderer>())
                    {
                        if (!bs.ContainsKey(eachMR.gameObject.name))
                        {
                            bs[eachMR.gameObject.name] = 0;
                        }

                        bs[eachMR.gameObject.name] += 1;
                    }

                    PrefabUtility.UnloadPrefabContents(t);
                }

                foreach (var h in bs)
                {
                    Audit.Append($"{h.Key}\t{h.Value.ToString()}\n");
                }

                TextAsset TA = new TextAsset(Audit.ToString());
                AssetDatabase.CreateAsset(TA, AssetDatabase.GetAssetPath(PrefabFolder) + "/audit.asset");
                AssetDatabase.SaveAssets();
            }
        }

        [MenuItem("Tools/MegacityMetro/Update Block Selected")]
        public static void UpdateBlockDefinition()
        {
            var blocks =
                AssetDatabase.LoadAssetAtPath<BlockDefinition>("Assets/Art/Models/Placeholder/WFC/Blocks.asset");
            var guidSelection = Selection.assetGUIDs;

            List<Block> newBlocks = new List<Block>();
            foreach (var guid in guidSelection)
            {
                var assetName = AssetDatabase.GUIDToAssetPath(guid);
                var thisPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetName);
                var fileName = System.IO.Path.GetFileName(assetName);
                char xx = fileName[6];
                char yy = fileName[8];
                int.TryParse(xx.ToString(), out int x);
                int.TryParse(yy.ToString(), out int y);
                var b = new Block
                {
                    Footprint = new int2(x, y),
                    PrefabName = thisPrefab
                };

                newBlocks.Add(b);
            }

            blocks.Blocks = newBlocks.ToArray();
            AssetDatabase.SaveAssets();
        }

        private void GenerateBlock(int seed)
        {
            PrefabSet.Clear();
            var blockName = $"Block_{seed}";
            Random.InitState(seed);
            var airShaft = Random.value < AirshaftChance;

            foreach (var eachPrefab in BlockDefinition.Blocks)
            {
                if (!PrefabSet.ContainsKey(eachPrefab.Footprint))
                {
                    PrefabSet[eachPrefab.Footprint] = new List<GameObject>();
                }
                PrefabSet[eachPrefab.Footprint].Add(eachPrefab.PrefabName);
            }

            var block = new GameObject(blockName)
            {
                transform = {position = Vector3.zero}
            };

            var result = Populate(airShaft, AllowIsolated);
            foreach (KeyValuePair<(int, int), int2> keyValuePair in result)
            {
                var (address, value) = keyValuePair;
                var pf = Random.Range(0, PrefabSet[value].Count);
                var proto = PrefabSet[value][pf];
                var go = (GameObject) PrefabUtility.InstantiatePrefab(proto, block.transform);

                Vector3 orig = new Vector3(address.Item1, 0, address.Item2) * FinalScale;
                Vector3 offset = new Vector3(value.x, 0, value.y) * (FinalScale / 2f);

                go.transform.position = offset + orig;
                go.transform.localScale = new Vector3(1, Random.Range(0.85f, 1.15f), 1);
            }
        }

        [MenuItem("Tools/MegacityMetro/BlockGenerator")]
        public static void ShowWindow()
        {
            var e = GetWindow<BlockWFC>();
            e.BlockDefinition =
                AssetDatabase.LoadAssetAtPath<BlockDefinition>("Assets/Art/Models/Placeholder/WFC/Blocks.asset");
            ((EditorWindow) e).Show();
        }

        public Dictionary<(int, int), int2> Populate(bool airShaft = true, bool allowIsolated = false)
        {
            var result = new Dictionary<(int, int), int2>();
            var candidates = PrefabSet.Keys.ToArray();

            var grid = new int2[5, 5];
            var negate = new int2(-1, -1);

            for (int i = 0; i < 5; i++)
            for (int j = 0; j < 5; j++)
            {
                grid[i, j] = negate;
            }

            if (airShaft)
            {
                var x = Random.Range(1, 2);
                var y = Random.Range(1, 2);
                var xx = Random.Range(1, 2);
                var yy = Random.Range(1, 2);
                for (int ax = x; ax < x + xx; ax++)
                for (int ay = y; ay < y + yy; ay++)
                {
                    grid[ax, ay] = new int2(0, 0);
                }
            }

            bool _fit(int x, int y, int2 b, int dx = 1, int dy = 1)
            {
                for (var i = x; i < x + b.x; i += dx)
                for (var j = y; j < y + b.y; j += dy)
                {
                    if ((i > 4) | (i < 0)) return false;
                    if ((j > 4) | (j < 0)) return false;
                    if (grid[i, j].x >= 0) return false;
                }

                bool xGrounded = (x == 0 || x + b.x == 5);
                bool yGrounded = (y == 0 || y + b.y == 5);
                if (!(xGrounded || yGrounded || allowIsolated)) return false;

                for (var i = x; i < x + b.x; i += dx)
                for (var j = y; j < y + b.y; j += dy)
                {
                    grid[i, j] = b;
                }

                return true;
            }

            int failsafe = 0;
            while (failsafe < 2048 && !_IsFull())
            {
                failsafe++;


                int x = -1;
                int y = -1;
                _NextAddress(ref x, ref y);
                if (x < 0 || y < 0) break;
                var b = candidates[Random.Range(1, candidates.Length)];
                if (failsafe < 4)
                {
                    while (b.x + b.y < 3)
                    {
                        b = candidates[Random.Range(1, candidates.Length)];
                    }
                }

                if (_fit(x, y, b))
                {
                    result[(x, y)] = b;
                }
            }

            if (!_IsFull())
            {
                var single = new int2(1, 1);
                for (var x = 0; x < 5; x++)
                {
                    if (grid[x, 0].x < 0) result[(x, 0)] = single;
                    if (grid[x, 4].x < 0) result[(x, 4)] = single;
                    if (grid[0, x].x < 0) result[(0, x)] = single;
                    if (grid[4, x].x < 0) result[(4, x)] = single;
                }
            }

            bool _IsFull()
            {
                for (int i = 0; i < 5; i++)
                for (int j = 0; j < 5; j++)
                {
                    if (grid[i, j].x < 0) return false;
                }

                return true;
            }

            void _NextAddress(ref int x, ref int y)
            {
                x = -1;
                y = -1;
                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        if (grid[i, j].x < 0)
                        {
                            x = i;
                            y = j;
                            return;
                        }
                    }
                }
            }

            return result;
        }
    }
}