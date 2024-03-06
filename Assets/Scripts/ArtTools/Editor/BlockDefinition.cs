using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "BlockData", menuName = "MegacityMetro/BlockDefinition", order = 1)]
public class BlockDefinition : ScriptableObject
{
    public Block[] Blocks;
}

[System.Serializable]
public struct Block
{
    public GameObject PrefabName;
    public int2 Footprint;
}