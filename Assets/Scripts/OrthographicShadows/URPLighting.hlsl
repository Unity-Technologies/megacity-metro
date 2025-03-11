#ifndef URPLIGHTING_INCLUDED
#define URPLIGHTING_INCLUDED

void URPLighting_float (out half3 direction, out half3 color, out half distanceAttenuation, out half shadowAttenuation)
{
    #ifdef SHADERGRAPH_PREVIEW
   
    direction = half3(-0.3, -0.8, 0.6);
    color = half3(1, 1, 1);
    distanceAttenuation = 1.0;
    shadowAttenuation = 1.0;
    #else

    Light mainLight = GetMainLight();
    direction = -mainLight.direction;
    color = mainLight.color;
    distanceAttenuation = mainLight.distanceAttenuation;
    shadowAttenuation = mainLight.shadowAttenuation;
   
    #endif
}

#endif
