#ifndef CUSTOM_LIGHTING
#define CUSTOM_LIGHTING

void MainLight_float(float3 worldPos, out float3 direction, out float3 color, out float shadowAtten)
{
#ifdef SHADERGRAPH_PREVIEW
    direction = normalize(float3(-0.5,0.5,-0.5));
    color = float3(1,1,1);
    shadowAtten = 1;
#else
    float4 shadowCoord = TransformWorldToShadowCoord(worldPos);
    Light mainLight = GetMainLight(shadowCoord);
    direction = mainLight.direction;
    color = mainLight.color;// *mainLight.distanceAttenuation;
    shadowAtten = mainLight.shadowAttenuation;
#endif
}

void MainLight_half(half3 worldPos, out half3 direction, out half3 color, out half shadowAtten)
{
#ifdef SHADERGRAPH_PREVIEW
    direction = normalize(half3(-0.5,0.5,0.5));
    color = half3(1,1,1);
    shadowAtten = 1;
#else
    half4 shadowCoord = TransformWorldToShadowCoord(worldPos);
    Light mainLight = GetMainLight(shadowCoord);
    direction = mainLight.direction;
    color = mainLight.color;// *mainLight.distanceAttenuation;
    shadowAtten = mainLight.shadowAttenuation;
#endif
}

void MainLightShadowFade_half(half3 worldPos, out half fade)
{
    #ifdef SHADERGRAPH_PREVIEW
    fade = 1;
    #else
    fade = GetMainLightShadowFade(worldPos);
    #endif
}

#ifndef SHADERGRAPH_PREVIEW

// This function gets additional light data and calculates realtime shadows
Light GetAdditionalLightCustom(int pixelLightIndex, float3 worldPosition) {
    // Convert the pixel light index to the light data index
    #if USE_FORWARD_PLUS
        int lightIndex = pixelLightIndex;
    #else
        int lightIndex = GetPerObjectLightIndex(pixelLightIndex);
    #endif
    // Call the URP additional light algorithm. This will not calculate shadows, since we don't pass a shadow mask value
    Light light = GetAdditionalPerObjectLight(lightIndex, worldPosition);
    // Manually set the shadow attenuation by calculating realtime shadows
    light.shadowAttenuation = AdditionalLightRealtimeShadow(lightIndex, worldPosition, light.direction);
    return light;
}

#endif

void AddAdditionalLights_float(float Smoothness, float3 WorldPosition, float3 WorldNormal, float3 WorldView,
    float MainDiffuse, float3 MainSpecular, float3 MainColor,
    out float Diffuse, out float3 Specular, out float3 Color) {

    Diffuse = MainDiffuse;
    Specular = MainSpecular;
    Color = MainColor * (MainDiffuse + MainSpecular);

#ifndef SHADERGRAPH_PREVIEW
    
    uint pixelLightCount = GetAdditionalLightsCount();

    // for Foward+ LIGHT_LOOP_BEGIN macro uses inputData.normalizedScreenSpaceUV and inputData.positionWS
    InputData inputData = (InputData)0;
    float4 screenPos = ComputeScreenPos(TransformWorldToHClip(WorldPosition));
    inputData.normalizedScreenSpaceUV = screenPos.xy / screenPos.w;
    inputData.positionWS = WorldPosition;

    LIGHT_LOOP_BEGIN(pixelLightCount)
        Light light = GetAdditionalLightCustom(lightIndex, WorldPosition);
        float NdotL = saturate(dot(WorldNormal, light.direction));
        float atten = light.distanceAttenuation * light.shadowAttenuation;
        float thisDiffuse = atten * NdotL;
        //float3 thisSpecular = LightingSpecular(thisDiffuse, light.direction, WorldNormal, WorldView, 1, Smoothness).x;
        Diffuse += thisDiffuse;
        //Specular += thisSpecular;
        //Color += light.color * (thisDiffuse + thisSpecular);
        float3 cookieColor = SampleAdditionalLightCookie(lightIndex, WorldPosition);
        Color += light.color * thisDiffuse * cookieColor;
        
        LIGHT_LOOP_END

    float total = Diffuse + dot(Specular, float3(0.333, 0.333, 0.333));
    Color = total <= 0 ? MainColor : Color / max(total, FLT_MIN);
#endif
}

void AddAdditionalLights_half(half Smoothness, half3 WorldPosition, half3 WorldNormal, half3 WorldView,
    half MainDiffuse, half3 MainSpecular, half3 MainColor,
    out half Diffuse, out half3 Specular, out half3 Color) {

    Diffuse = MainDiffuse;
    Specular = MainSpecular;
    Color = MainColor * (MainDiffuse + MainSpecular);

#ifndef SHADERGRAPH_PREVIEW

    uint pixelLightCount = GetAdditionalLightsCount();

    // for Foward+ LIGHT_LOOP_BEGIN macro uses inputData.normalizedScreenSpaceUV and inputData.positionWS
    InputData inputData = (InputData)0;
    float4 screenPos = ComputeScreenPos(TransformWorldToHClip(WorldPosition));
    inputData.normalizedScreenSpaceUV = screenPos.xy / screenPos.w;
    inputData.positionWS = WorldPosition;

    LIGHT_LOOP_BEGIN(pixelLightCount)
        Light light = GetAdditionalLightCustom(lightIndex, WorldPosition);
        half NdotL = saturate(dot(WorldNormal, light.direction));
        half atten = light.distanceAttenuation * light.shadowAttenuation;
        half thisDiffuse = atten * NdotL;
        //half3 thisSpecular = LightingSpecular(thisDiffuse * light.color, light.direction, WorldNormal, WorldView, 1, Smoothness).x;
        Diffuse += thisDiffuse;
        //Specular += thisSpecular;
        //Color += light.color * (thisDiffuse + thisSpecular);
        half3 cookieColor = SampleAdditionalLightCookie(lightIndex, WorldPosition);
        Color += light.color * thisDiffuse * cookieColor;
        LIGHT_LOOP_END
    //needs to be float to avoid precision issues
    float total = Diffuse + dot(Specular, half3(0.333, 0.333, 0.333));
    Color = total <= 0 ? MainColor : Color / max(total, HALF_MIN);
#endif
}

#endif