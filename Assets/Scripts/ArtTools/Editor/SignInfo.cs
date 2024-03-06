using UnityEngine;

[CreateAssetMenu(fileName = "SignLayout", menuName = "MegacityMetro/SignLayout", order = 1)]
public class SignInfo : ScriptableObject
{
    public Vector2 BlockSize;
    public float StreetSize;
    public Vector2Int SignsPerBlockRange;
    public SignPlacer.SignItem[] Items;
}