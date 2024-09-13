using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Rendering.Universal;

[ExecuteAlways]
[RequireComponent(typeof(DirectionalLight))]
public class OrthographicShadow : MonoBehaviour
{
    public enum ShadowMapType
    {
        Desktop,
        MobileHigh,
        MobileLow
    }

    public UniversalRenderPipelineAsset CaptureRenderer;
    
    [Header("Shadow Map")]
    [SerializeField]
    public Texture2D ShadowMap;
    public Vector2Int ShadowMapResolution = new Vector2Int(1024, 1024);
    public ShadowMapType MapType = ShadowMapType.Desktop;

    [Header("Projection")]
    public float Width = 200f;
    public float Height = 200f;
    public float Depth = 1000f;
    public float DepthOffset;

    [Header("Shader")]
    public float MobileShadowBias = 15f;
    public float DesktopShadowBias = 10f;
    
    
    Vector3 m_Gizmo;
    Transform m_LocalTransform;
    Matrix4x4 m_OrthoProjectionMatrix;
    static readonly int k_OrthographicShadowMatrix = Shader.PropertyToID("_OrthographicShadowMatrix");
    static readonly int k_OrthographicShadowMatrixInv = Shader.PropertyToID("_OrthographicShadowMatrixInv");
    static readonly int k_OrthographicProjectionVolume = Shader.PropertyToID("_OrthographicProjectionVolume");
    static readonly int k_OrthographicShadowTexture = Shader.PropertyToID("_OrthographicShadowTexture");
    static readonly int k_OrthographicShadowFadeDistance = Shader.PropertyToID("_OrthographicShadowFadeDistance");
    static readonly int k_OrthographicShadowActive = Shader.PropertyToID("_OrthographicShadowActive");
    static readonly int k_OrthographicShadowBiasDesktop = Shader.PropertyToID("_OrthographicShadowBiasDesktop");
    static readonly int k_OrthographicShadowBiasMobile = Shader.PropertyToID("_OrthographicShadowBiasMobile");


    void Awake()
    {
        SetRuntimeConstants();
  
    }

    void OnEnable()
    {
        Shader.SetGlobalFloat(k_OrthographicShadowActive, 1f);
        m_LocalTransform = gameObject.transform;
    }

    void OnDisable()
    {
        Shader.SetGlobalFloat(k_OrthographicShadowActive, 0f);
    }

    void OnDrawGizmos()
    {
        m_LocalTransform ??= gameObject.transform;
        Gizmos.color = new Color(0, 1, 0, 0.2f);
        Gizmos.matrix = m_LocalTransform.localToWorldMatrix;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(Vector3.forward * (Depth / 2) + (Vector3.forward * DepthOffset), m_Gizmo);
    }

    void OnValidate()
    {
        m_Gizmo.x = Width;
        m_Gizmo.y = Height;
        m_Gizmo.z = Depth;
    }

    /// <summary>
    /// This updates the shader constants with the info from the most recent capture.
    /// </summary>
    public void SetRuntimeConstants()
    {
        m_LocalTransform ??= gameObject.transform;
        var urpAsset = UniversalRenderPipeline.asset;
        var shadowFadeDistance = urpAsset.shadowDistance * (1 - urpAsset.cascadeBorder);
        var p = m_LocalTransform.localToWorldMatrix.MultiplyPoint(new Vector3(
            -0.5f * Width, -0.5f * Height, DepthOffset));

        var x = m_LocalTransform.right * Width;
        var y = m_LocalTransform.up * Height;
        var z = m_LocalTransform.forward * Depth;

        m_OrthoProjectionMatrix = Matrix4x4.identity;
        m_OrthoProjectionMatrix.SetColumn(0, new Vector4(x.x, x.y, x.z, 0));
        m_OrthoProjectionMatrix.SetColumn(1, new Vector4(y.x, y.y, y.z, 0));
        m_OrthoProjectionMatrix.SetColumn(2, new Vector4(z.x, z.y, z.z, 0));
        m_OrthoProjectionMatrix.SetColumn(3, new Vector4(p.x, p.y, p.z, 1));

        Shader.SetGlobalMatrix(k_OrthographicShadowMatrix, m_OrthoProjectionMatrix);
        Shader.SetGlobalMatrix(k_OrthographicShadowMatrixInv, m_OrthoProjectionMatrix.inverse.transpose);
        Shader.SetGlobalVector(k_OrthographicProjectionVolume, new Vector4(Width, Height, Depth, DepthOffset));
        Shader.SetGlobalTexture(k_OrthographicShadowTexture, ShadowMap);
        Shader.SetGlobalFloat(k_OrthographicShadowFadeDistance, shadowFadeDistance);
        Shader.SetGlobalFloat(k_OrthographicShadowBiasDesktop, DesktopShadowBias);
        Shader.SetGlobalFloat(k_OrthographicShadowBiasMobile, MobileShadowBias);

    }

   }
