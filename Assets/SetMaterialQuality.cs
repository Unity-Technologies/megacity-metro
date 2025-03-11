using UnityEngine;
using UnityEngine.Rendering;

public class SetMaterialQuality : MonoBehaviour
{

    public MaterialQuality materialQuality;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        materialQuality.SetGlobalShaderKeywords();
    }
}
