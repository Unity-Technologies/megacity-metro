using UnityEditor;
using UnityEngine;

public class DepthCapture : EditorWindow
{
    [MenuItem("Tools/MegacityMetro/Convert Depth Texture")]
    public static void ConvertDepthTexture()
    {
        var q = QualitySettings.GetQualityLevel();
        QualitySettings.SetQualityLevel(3);

        var crt = AssetDatabase.LoadAssetAtPath<RenderTexture>(
            "Assets/Art/CustomLighting/BakedShadowMap/rawDepthCapture.renderTexture");

        Debug.Log(crt);
        var T2d = new Texture2D(2048, 1024, TextureFormat.RGBA32, false);
        RenderTexture.active = crt;

        T2d.ReadPixels(new Rect(0, 512, 2048, 1024), 0, 0, false);
        T2d.Apply();

        Debug.Log("collected pixels");
        var bytes = T2d.EncodeToEXR(Texture2D.EXRFlags.OutputAsFloat);

        System.IO.File.WriteAllBytes(
            Application.dataPath + "/Art/CustomLighting/BakedShadowMap/BakedShadowMap_capture_3.exr", bytes);

        QualitySettings.SetQualityLevel(q);
    }
}