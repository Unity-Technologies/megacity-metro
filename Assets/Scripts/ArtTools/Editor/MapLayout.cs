using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "MapLayout", menuName = "MegacityMetro/MapLayout", order = 1)]
public class MapLayout : ScriptableObject
{
    public Vector2 BlockSize;
    public float StreetSize;
    public int2 Blocks;
    public int2[] Knockouts;
}