void isAndroid_float(out bool result)
{
    result = false;

    #ifdef SHADER_API_VULKAN
    result =  true;
    #endif
}

void isAndroid_half(out bool result)
{
    result = false;

    #ifdef SHADER_API_MOBILE
    result =  true;
    #endif
}