#ifndef CUSTOM_VOLUMETRICS
#define CUSTOM_VOLUMETRICS

#include "Assets/Scripts/OrthographicShadows/DistantShadows.hlsl"

/*
float OldSampleStaticShadowMap(float3 pos, UnityTexture2D staticShadowMap, UnitySamplerState samplerState)
{
    float3 cameraPosition = float3(-910, 697, -2920);

    float3x3 cameraRotation =
    {
        0.9571005,0.02300199,0.2888417,
        0.03756225,0.9785833,-0.2023953,
        -0.2873112,0.2045622,0.9357402
    };

    float2 nearFarPlane = float2(450, 5600);

    float3 shadowCoordParams = pos - cameraPosition;
    shadowCoordParams= mul(shadowCoordParams, cameraRotation);

    float shadowDepth = (shadowCoordParams.z - nearFarPlane.x)/nearFarPlane.y;
    shadowDepth = lerp(1,0,shadowDepth);

    //bias
    shadowDepth -= 0.03;
    
    float2 orthoParams = float2(4,2) * 1400;

    float2 shadowCoords = (shadowCoordParams.xy / orthoParams) + float2(0.5,0.5);
    shadowCoords = float2(shadowCoords.x, 1-shadowCoords.y);

    float sample = SAMPLE_TEXTURE2D(staticShadowMap, samplerState, shadowCoords);

    return step(sample, shadowDepth);
}
*/

float distSquared( float3 A, float3 B )
{
    float3 C = A - B;
    return dot(C,C);
}

void Volumetrics_float(int steps, float distance, float2 screenUV, float3 cameraPos, float3 fragmentPos, float3 viewDirection, UnityTexture2D staticShadowMap, UnityTexture3D densityMap, UnitySamplerState samplerState, out float godrays, out float noise)
{
#ifdef SHADERGRAPH_PREVIEW
    godrays = 0;
    noise = 0;
#else
    
    float endDist = distance;

    float divSteps = 1.0 / steps;
    
    float stepDist = endDist * divSteps;

    float fragmentDepth = distSquared(fragmentPos , cameraPos);

    float godRayResult = 0;
    float noiseResult = 0;

#ifdef USE_BAYER_OFFSET
    float4x4 bayerMatrix =
    {
        0, 8, 2, 10,
        12, 4, 14, 6,
        3, 11, 1, 9,
        15, 7, 13, 5
    };

    int pixelX = screenUV.x * _ScaledScreenParams.x % 4;
    int pixelY = screenUV.y * _ScaledScreenParams.y % 4;
    
    float sampleOffset = bayerMatrix[pixelX][pixelY] / 16.0;
#else
    float sampleOffset = 0;
#endif
    
    
    for(int i = 0; i < steps; i++)
    {
        
        float3 samplePos = cameraPos + viewDirection * stepDist * (i+sampleOffset);
        float sampleDepth = distSquared(samplePos, cameraPos);
        
        float normalizedSampleDepth = 1 - saturate(float(i) * divSteps);


        //This code samples real time shadow map. Use if no static map is available
        //float4 shadowCoord = TransformWorldToShadowCoord(samplePos);
        //half shadow = MainLightRealtimeShadow(shadowCoord);

        //This code samples static shadow map
        float shadow = SampleStaticShadowMap(samplePos);
        
        float zOffset = saturate(samplePos.y/2000);
        
        float3 noiseUV1 = samplePos * 0.001 + float3(-_Time.g*0.005,-_Time.g*0.01, zOffset);
        
        float fog = SAMPLE_TEXTURE3D(densityMap, samplerState, noiseUV1);
        fog = fog*0.9 + 0.1;
        fog *= fog;
        fog *= 2;
        fog *= 1 - saturate(samplePos.y / 500);
        
        float depthClip = sampleDepth > fragmentDepth ? 0 : 1;
        
        godRayResult += shadow * depthClip * normalizedSampleDepth;
        noiseResult += fog * depthClip * normalizedSampleDepth;

    }

    godrays = godRayResult * divSteps;
    noise = noiseResult * divSteps;
    #endif
}

#endif