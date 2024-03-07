using Unity.Entities;
using UnityEngine;

public static class RateSettings
{
    public const int framerate = -1;
    public const int tickRate = 30;
    public const int fixedRateRatio = 1;

    public static bool UnlockFramerate;        
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void ResetStaticData()
    {
        UnlockFramerate = false;
    }
    
    public static float GetFixedTimeStep()
    {
        return 1f / (tickRate * fixedRateRatio);
    }
    
    public static void ApplyRates(FixedStepSimulationSystemGroup group)
    {
        ApplyFrameRate();
        if (group != null)
        {
            group.Timestep = GetFixedTimeStep();
        }
    }

    public static void ApplyFrameRate()
    {
        if (UnlockFramerate)
        {
            QualitySettings.vSyncCount = 0;
#if UNITY_ANDROID || UNITY_IPHONE
            Application.targetFrameRate = 120;
#else
            Application.targetFrameRate = -1;
#endif
        }
        else
        {
            Application.targetFrameRate = framerate;
        }
    }
}
