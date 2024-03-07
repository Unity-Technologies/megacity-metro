#ifndef _SHADER_GLOBALS_SAMPLE_SOURCE
#define _SHADER_GLOBALS_SAMPLE_SOURCE

float4 _gEastColor;
float4 _gWestColor;
float4 _gNorthColor;
float4 _gSouthColor;
float4 _gTopColor;
float _gGradientRadius;
float _gHeightMin;
float _gHeightMax;
float _gHeightFalloff;
float _gHeightOpacity;
float4 _gHeightColor;
float _gDistanceMin;
float _gDistanceMax;
float _gDistanceFalloff;
float _gDistanceOpacity;
TEXTURECUBE(_gDistanceCube);
SAMPLER(sampler_gDistanceCube);

void Unity_ColorspaceConversion_RGB_Linear_float(float3 In, out float3 Out)
{
    float3 linearRGBLo = In / 12.92;;
    float3 linearRGBHi = pow(max(abs((In + 0.055) / 1.055), 1.192092896e-07), float3(2.4, 2.4, 2.4));
    Out = float3(In <= 0.04045) ? linearRGBLo : linearRGBHi;
}

void Unity_ColorspaceConversion_RGB_Linear_half(half3 In, out half3 Out)
{
    half3 linearRGBLo = In / 12.92;;
    half3 linearRGBHi = pow(max(abs((In + 0.055) / 1.055), 1.192092896e-07), half3(2.4, 2.4, 2.4));
    Out = half3(In <= 0.04045) ? linearRGBLo : linearRGBHi;
}

void GetShaderGlobals_float(out float3 EastColor, out float3 WestColor, out float3 NorthColor, out float3 SouthColor, 
    out float3 TopColor, out float GradientRadius, out float HeightMin, out float HeightMax, out float HeightFalloff, out float HeightOpacity, 
    out float3 HeightColor, out float DistanceMin, out float DistanceMax, out float DistanceFalloff, out float DistanceOpacity, out UnityTextureCube DistanceCube)
{
    Unity_ColorspaceConversion_RGB_Linear_float(_gEastColor.rgb, EastColor);
    Unity_ColorspaceConversion_RGB_Linear_float(_gWestColor.rgb, WestColor);
    Unity_ColorspaceConversion_RGB_Linear_float(_gNorthColor.rgb, NorthColor);
    Unity_ColorspaceConversion_RGB_Linear_float(_gSouthColor.rgb, SouthColor);
    Unity_ColorspaceConversion_RGB_Linear_float(_gTopColor.rgb, TopColor);
    GradientRadius = _gGradientRadius;
    HeightMin = _gHeightMin;
    HeightMax = _gHeightMax;
    HeightFalloff = _gHeightFalloff;
    HeightOpacity = _gHeightOpacity;
    Unity_ColorspaceConversion_RGB_Linear_float(_gHeightColor.rgb, HeightColor);
    DistanceMin = _gDistanceMin;
    DistanceMax = _gDistanceMax;
    DistanceFalloff = _gDistanceFalloff;
    DistanceOpacity = _gDistanceOpacity;
    DistanceCube = UnityBuildTextureCubeStruct(_gDistanceCube);
}

void GetShaderGlobals_half(out half3 EastColor, out half3 WestColor, out half3 NorthColor, out half3 SouthColor,
    out half3 TopColor, out half GradientRadius, out half HeightMin, out half HeightMax, out half HeightFalloff, out half HeightOpacity,
    out half3 HeightColor, out half DistanceMin, out half DistanceMax, out half DistanceFalloff, out half DistanceOpacity, out UnityTextureCube DistanceCube)
{
    Unity_ColorspaceConversion_RGB_Linear_half(_gEastColor.rgb, EastColor);
    Unity_ColorspaceConversion_RGB_Linear_half(_gWestColor.rgb, WestColor);
    Unity_ColorspaceConversion_RGB_Linear_half(_gNorthColor.rgb, NorthColor);
    Unity_ColorspaceConversion_RGB_Linear_half(_gSouthColor.rgb, SouthColor);
    Unity_ColorspaceConversion_RGB_Linear_half(_gTopColor.rgb, TopColor);
    GradientRadius = _gGradientRadius;
    HeightMin = _gHeightMin;
    HeightMax = _gHeightMax;
    HeightFalloff = _gHeightFalloff;
    HeightOpacity = _gHeightOpacity;
    Unity_ColorspaceConversion_RGB_Linear_half(_gHeightColor.rgb, HeightColor);
    DistanceMin = _gDistanceMin;
    DistanceMax = _gDistanceMax;
    DistanceFalloff = _gDistanceFalloff;
    DistanceOpacity = _gDistanceOpacity;
    DistanceCube = UnityBuildTextureCubeStruct(_gDistanceCube);
}

#endif
