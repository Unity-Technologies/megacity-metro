using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public partial struct AnimatedTimeSystem : ISystem
{

    void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<AnimatedTimeSingleton>();
    }
    
    void OnUpdate(ref SystemState state)
    {
        AnimatedTime animatedTime = state.EntityManager.GetComponentObject<AnimatedTime>(SystemAPI.GetSingletonEntity<AnimatedTimeSingleton>());
        
        float4 timeVector = default;
        float4 hitchVector = default;
        
        timeVector.x = Time.time % 8f;
        timeVector.y = Time.time % 16f;
        timeVector.z = Time.time % 32f;
        timeVector.w = Time.time % 64f;
        
        hitchVector.x = Mathf.Abs((timeVector.x / 8) - 0.5f) *2;
        hitchVector.y = Mathf.Abs((timeVector.y / 16) - 0.5f) *2;
        hitchVector.z = Mathf.Abs((timeVector.z / 32) - 0.5f) *2;
        hitchVector.w = Mathf.Abs((timeVector.w / 64) - 0.5f) *2;


        foreach (var material in animatedTime.Materials)
        {
            material.SetVector("_Time16", timeVector);
            material.SetVector("_Time16Hitch", hitchVector);
        }
        
    }
}
