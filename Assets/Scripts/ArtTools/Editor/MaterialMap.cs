using UnityEngine;

[CreateAssetMenu(fileName = "MaterialAssignments", menuName = "MegacityMetro/Material Assignment Map", order = 2)]

public class MaterialMap : ScriptableObject
{
    public MaterialAssignment[] Assignments;
}

[System.Serializable]
public struct MaterialAssignment
{
    public string MaterialSlot;
    public Material Material;
}