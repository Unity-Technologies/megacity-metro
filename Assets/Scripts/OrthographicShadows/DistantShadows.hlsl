#ifndef DISTANT_SHADOWS_DEFINED
#define DISTANT_SHADOWS_DEFINED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/GlobalSamplers.hlsl"

// this function exists to access the shadow matrix and texture globals
// without requiring special setup in the shader

float4x4 _OrthographicShadowMatrix;
float4x4 _OrthographicShadowMatrixInv;
Texture2D _OrthographicShadowTexture;
sampler sampler_OrthographicShadowTexture;
float4 _OrthographicShadowTexture_TexelSize;
float4 _OrthographicShadowTexture_ST;
float _OrthographicShadowFadeDistance = 100;
float _OrthographicShadowActive = 1;
float _OrthographicShadowBiasDesktop = 10;
float _OrthographicShadowBiasMobile = 15;


void DistantShadows_float(
    out float4x4 ShadowMatrix,
    out float4x4 ShadowMatrixInv,
    out float ShadowFadeDistance,
    out float ShadowActive,
    out UnityTexture2D ShadowTexture,
    out float ShadowBiasDesktop,
    out float ShadowBiasMobile)
{
    ShadowMatrix = _OrthographicShadowMatrix;
    ShadowMatrixInv = _OrthographicShadowMatrixInv;
    ShadowFadeDistance = _OrthographicShadowFadeDistance;
    ShadowActive =  _OrthographicShadowActive;
    ShadowTexture = UnityBuildTexture2DStruct(_OrthographicShadowTexture);
    ShadowBiasDesktop = _OrthographicShadowBiasDesktop;
    ShadowBiasMobile = _OrthographicShadowBiasMobile;
}

float3 WorldPosToStaticShadowCoord(float3 worldPos)
{
    float4 coord = mul(float4(worldPos.x, worldPos.y, worldPos.z, 1), _OrthographicShadowMatrixInv);
    return coord.xyz;
}


float SampleStaticShadowMap(float3 worldPos)
{
    //SamplerState linear_clamp_sampler;
    float3 shadowCoord = WorldPosToStaticShadowCoord(worldPos);
    float sample = SAMPLE_TEXTURE2D(_OrthographicShadowTexture, sampler_LinearClamp, shadowCoord.xy);

    float shadowDepth = (1-shadowCoord.z) + 0.005; //Should maybe be replaced with proper biasing
    
    if(sample - shadowDepth < 0)
    {
        return 1;
    } else
    {
        return 0;
    }
}


#endif