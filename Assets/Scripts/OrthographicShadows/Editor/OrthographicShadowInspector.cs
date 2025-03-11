using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.UI;
using UnityEngine.Rendering.Universal;

#if UNITY_EDITOR

[CustomEditor(typeof(OrthographicShadow))]
class OrthographicShadowInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Capture"))
        {
            var tgt = (OrthographicShadow)target;
            UpdateShadowMap(tgt);
            tgt.SetRuntimeConstants();
        }
    }
    
    
    /// <summary>
    /// Capture the shadow map and compress it using the user-supplied settings
    /// </summary>
    /// <param name="orthographicShadow"></param>
    static void UpdateShadowMap(OrthographicShadow orthographicShadow)
    {
        var captureFormat = TextureFormat.BC4;
        switch (orthographicShadow.MapType)
        {
            case OrthographicShadow.ShadowMapType.Desktop:
                captureFormat = TextureFormat.RHalf;
                break;
            case OrthographicShadow.ShadowMapType.MobileHigh:
                captureFormat = TextureFormat.ASTC_HDR_4x4;
                break;
            case OrthographicShadow.ShadowMapType.MobileLow:
                captureFormat = TextureFormat.ASTC_HDR_10x10;
                break;
        }
        var orthoCamera = orthographicShadow.gameObject.AddComponent<Camera>();
        orthoCamera.orthographic = true;
        orthoCamera.depthTextureMode = DepthTextureMode.Depth;
        orthoCamera.aspect = orthographicShadow.Width / orthographicShadow.Height;
        orthoCamera.orthographicSize = orthographicShadow.Height / 2;
        orthoCamera.nearClipPlane = orthographicShadow.DepthOffset;
        orthoCamera.farClipPlane = orthographicShadow.DepthOffset + orthographicShadow.Depth;
        orthoCamera.clearFlags = CameraClearFlags.Depth;
        var additionalCamData = orthoCamera.GetUniversalAdditionalCameraData();

        additionalCamData.renderShadows = false;
        additionalCamData.requiresColorOption = CameraOverrideOption.Off;
        additionalCamData.requiresDepthOption = CameraOverrideOption.On;
        additionalCamData.SetRenderer(0);


        var oldSettings = QualitySettings.GetQualityLevel();

        try
        {
        
            QualitySettings.SetQualityLevel(3, true);
            CaptureDepthTexture(orthographicShadow, orthoCamera, captureFormat);
        }
        finally
        {
          
            if (Application.isEditor)
            {
                DestroyImmediate(additionalCamData);
                DestroyImmediate(orthoCamera);
            }
            else
            {
                Destroy(additionalCamData);
                Destroy(orthoCamera);
            }
            
            QualitySettings.SetQualityLevel(oldSettings, true);
        }
        
        
    }

    /// <summary>
    ///     Render the scene through camera `cam` and returns a BC4 encoded depth map texture
    /// </summary>
    /// <param name="orthographicShadow"></param>
    /// <param name="cam">The camera to render</param>
    /// <returns>A BC4 encoded depth map</returns>
    static void CaptureDepthTexture(OrthographicShadow orthographicShadow, in Camera cam, TextureFormat format)
    {
        
        var rt = new RenderTexture(orthographicShadow.ShadowMapResolution.x, orthographicShadow.ShadowMapResolution.y, 32, RenderTextureFormat.Depth, RenderTextureReadWrite.Linear);
        rt.format = RenderTextureFormat.ARGBFloat;
        cam.targetTexture = rt;

        cam.Render();

        if (orthographicShadow.ShadowMap)
        {
            orthographicShadow.ShadowMap.Reinitialize(orthographicShadow.ShadowMapResolution.x, orthographicShadow.ShadowMapResolution.y, TextureFormat.RFloat, false);            
        }
        else
        {
            orthographicShadow.ShadowMap = new Texture2D(orthographicShadow.ShadowMapResolution.x, orthographicShadow.ShadowMapResolution.y, TextureFormat.RFloat, false);
            orthographicShadow.ShadowMap.name = "OrthoShadowMap";
            AssetDatabase.CreateAsset(orthographicShadow.ShadowMap, "Assets/OrthoShadow.asset");
            AssetDatabase.SaveAssets();
        }
        
        RenderTexture.active = rt;
        orthographicShadow.ShadowMap.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        orthographicShadow.ShadowMap.Apply();
        EditorUtility.CompressTexture(orthographicShadow.ShadowMap, format, TextureCompressionQuality.Best);

        // Clean up
        cam.targetTexture = null;
        RenderTexture.active = null;
        if (Application.isEditor)
        {
            DestroyImmediate(rt);
        }
        else
        {
            Destroy(rt);
        }
    }

}

#endif
