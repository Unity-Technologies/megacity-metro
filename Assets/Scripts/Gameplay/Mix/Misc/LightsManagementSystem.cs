using System.Collections;
using System.Collections.Generic;
using Gameplay.Misc.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Collider = Unity.Physics.Collider;
using Material = Unity.Physics.Material;
using SphereCollider = Unity.Physics.SphereCollider;


public struct LightTag : IComponentData
{
}

public struct LightIgnoreTag : IComponentData
{
}

public struct CullableLight : IComponentData
{
    public float OriginalIntensity;
    public float Range;
    public float TimeEnabled;
}

[UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
public partial struct LightsManagementSystem : ISystem
{
    public struct ManagedLightData
    {
        public Entity Entity;
        public ColliderKey Key;
        public float Distance;
    }
    
    private NativeList<ManagedLightData> _closestLights;
    private NativeList<ManagedLightData> _prevClosestLights;
    private EntityQuery _untaggedLightsQuery;
    
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GeneralSettings>();
        state.RequireForUpdate<PhysicsWorldSingleton>();

        _untaggedLightsQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Light>().WithNone<LightTag>().Build(ref state);
        
        _closestLights = new NativeList<ManagedLightData>(16, Allocator.Persistent);
        _prevClosestLights = new NativeList<ManagedLightData>(16, Allocator.Persistent);
    }
    
    public void OnDestroy(ref SystemState state)
    {
        if (_closestLights.IsCreated)
        {
            _closestLights.Dispose();
        }
        if (_prevClosestLights.IsCreated)
        {
            _prevClosestLights.Dispose();
        }
    }
    
    public void OnUpdate(ref SystemState state)
    {
        if (state.World.IsServer() || state.World.IsThinClient())
        {
            if (_untaggedLightsQuery.CalculateEntityCount() > 0)
            {
                state.EntityManager.DestroyEntity(_untaggedLightsQuery);
            }
        }
        else
        {
            if (Camera.main == null)
                return;
            
            GeneralSettings generalSettings = SystemAPI.GetSingleton<GeneralSettings>();
            
            LightsManagementJob job = new LightsManagementJob
            {
                GeneralSettings = generalSettings,
                CollisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld,
                ClosestLights = _closestLights,
                PhysicsColliderKeyEntityPairLookup = SystemAPI.GetBufferLookup<PhysicsColliderKeyEntityPair>(true),
                CameraPosition = Camera.main.transform.position,
                CameraForward = Camera.main.transform.forward,
            };
            state.Dependency = job.Schedule(state.Dependency);

            state.Dependency.Complete();

            float elapsedTime = (float)SystemAPI.Time.ElapsedTime;
            EntityCommandBuffer ecb = SystemAPI.GetSingletonRW<BeginSimulationEntityCommandBufferSystem.Singleton>().ValueRW.CreateCommandBuffer(state.WorldUnmanaged);

            // Tag lights with a component, so we can query them unmanagedly
            if (_untaggedLightsQuery.CalculateEntityCount() > 0)
            {
                state.EntityManager.AddComponent<LightTag>(_untaggedLightsQuery);
            }

            // Initialization +  disabling of lights
            foreach (var (lightTag, entity) in SystemAPI.Query<RefRO<LightTag>>().WithNone<CullableLight>().WithNone<LightIgnoreTag>().WithEntityAccess())
            {
                Light lightComponent = state.EntityManager.GetComponentObject<Light>(entity);
                if (lightComponent.type == LightType.Directional)
                {
                    ecb.AddComponent<LightIgnoreTag>(entity);
                }
                else
                {
                    lightComponent.gameObject.SetActive(false);
                    
                    ecb.AddComponent(entity, new PhysicsCollider
                    {
                        Value = generalSettings.LightsCollider,
                    });
                    ecb.AddSharedComponent(entity, new PhysicsWorldIndex(0));

                    ecb.AddComponent(entity, new CullableLight
                    {
                        OriginalIntensity = lightComponent.intensity,
                        Range = lightComponent.range,
                        TimeEnabled = 0f,
                    });
                }
            }

            // Enable new lights
            ComponentLookup<CullableLight> managedLightLookup = SystemAPI.GetComponentLookup<CullableLight>(false);
            for (int i = 0; i < _closestLights.Length; i++)
            {
                ManagedLightData data = _closestLights[i];

                if (data.Entity != Entity.Null && managedLightLookup.HasComponent(data.Entity))
                {
                    RefRW<CullableLight> managedLight = managedLightLookup.GetRefRW(data.Entity);
                    if (managedLight.IsValid)
                    {
                        Light light = state.EntityManager.GetComponentObject<Light>(data.Entity);

                        // Enable new lights
                        if (!IsInList(data.Entity, ref _prevClosestLights))
                        {
                            light.gameObject.SetActive(true);
                            managedLight.ValueRW.TimeEnabled = elapsedTime;
                        }

                        // Intensity Fade
                        float intensityRatio = math.saturate(math.max(elapsedTime - managedLight.ValueRW.TimeEnabled, 0f) / generalSettings.LightsFadeInDuration);
                        light.intensity = managedLight.ValueRW.OriginalIntensity * intensityRatio;
                    }
                }
            }

            // Disable old lights
            for (int i = 0; i < _prevClosestLights.Length; i++)
            {
                Entity entity = _prevClosestLights[i].Entity;
                if (!IsInList(entity, ref _closestLights))
                {
                    if (state.EntityManager.HasComponent<Light>(entity))
                    {
                        state.EntityManager.GetComponentObject<Light>(entity).gameObject.SetActive(false);
                    }
                }
            }

            _prevClosestLights.Clear();
            _prevClosestLights.AddRange(_closestLights.AsArray()); 
        }
    }

    private bool IsInList(Entity entity, ref NativeList<ManagedLightData> lightList)
    {
        for (int i = 0; i < lightList.Length; i++)
        {
            if (lightList[i].Entity == entity)
            {
                return true;
            }
        }
        return false;
    }

    [BurstCompile]
    public struct LightsManagementJob : IJob
    {
        public GeneralSettings GeneralSettings;
        [ReadOnly]
        public CollisionWorld CollisionWorld;
        public NativeList<ManagedLightData> ClosestLights;
        [ReadOnly]
        public BufferLookup<PhysicsColliderKeyEntityPair> PhysicsColliderKeyEntityPairLookup;
        public float3 CameraPosition;
        public float3 CameraForward;

        public void Execute()
        {
            PointDistanceInput pointInput = new PointDistanceInput
            {
                Filter = GeneralSettings.LightsDetectionFilter,
                Position = CameraPosition,
                MaxDistance = GeneralSettings.MaxLightsDetectionRange,
            };

            ClosestLights.Clear();
            
            ClosestLightsCollector collector = new ClosestLightsCollector();
            collector.MaxLights = GeneralSettings.MaxLights;
            collector.ViewForward = CameraForward;
            collector.MaxDistance = GeneralSettings.MaxLightsDetectionRange;
            collector.ForwardBias = GeneralSettings.ForwardBias;
            collector.MinDistanceForForwardBias = GeneralSettings.MinDistanceForForwardBias;
            collector.ClosestHits = ClosestLights;
            collector.MaxFraction = pointInput.MaxDistance;
            
            CollisionWorld.CalculateDistance(pointInput, ref collector);
            ClosestLights = collector.ClosestHits;
            
            // Reassign entities based on original collider entity
            for (int i = 0; i < ClosestLights.Length; i++)
            {
                ManagedLightData elem = ClosestLights[i];
                elem.Entity = FindOriginalColliderEntity(elem.Entity, elem.Key);
                ClosestLights[i] = elem;
            }
        }

        private Entity FindOriginalColliderEntity(Entity rootEntity, ColliderKey key)
        {
            if(PhysicsColliderKeyEntityPairLookup.TryGetBuffer(rootEntity, out DynamicBuffer<PhysicsColliderKeyEntityPair> buffer))
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    var elem = buffer[i];
                    if (key == elem.Key)
                    {
                        return elem.Entity;
                    }
                }
            }
            
            return rootEntity;
        }
    }

    public struct ClosestLightsCollector : ICollector<DistanceHit>
    {
        public bool EarlyOutOnFirstHit => false;
        public float MaxFraction { get; set; }
        public int NumHits => ClosestHits.Length;

        public int MaxLights;
        public float MaxDistance;
        public float ForwardBias;
        public float MinDistanceForForwardBias;
        public float3 ViewForward;
        public NativeList<ManagedLightData> ClosestHits;

        public bool AddHit(DistanceHit hit)
        {
            if (ClosestHits.Length < MaxLights)
            {
                ClosestHits.Add(new ManagedLightData
                {
                    Entity = hit.Entity,
                    Key = hit.ColliderKey,
                    Distance = GetBiasedDistance(hit),
                });
                return true;
            }

            // Find the current farthest hit
            int farthestIndex = -1;
            float farthestDistance = 0f;
            for (int i = 0; i < ClosestHits.Length; i++)
            {
                float tmpDist = ClosestHits[i].Distance;
                if (tmpDist > farthestDistance)
                {
                    farthestIndex = i;
                    farthestDistance = tmpDist;
                }
            }

            // If the new hit distance is closer than current farthest, replace
            float biasedDistance = GetBiasedDistance(hit);
            if (farthestIndex >= 0 && biasedDistance < farthestDistance)
            {
                ClosestHits[farthestIndex] = new ManagedLightData
                {
                    Entity = hit.Entity,
                    Key = hit.ColliderKey,
                    Distance = biasedDistance,
                };
                return true;
            }

            return false;
        }

        private float GetBiasedDistance(DistanceHit hit)
        {
            float dotRatio = math.dot(ViewForward, -hit.SurfaceNormal);
            if (dotRatio < 0f && hit.Distance > MinDistanceForForwardBias)
            {
                return hit.Distance * (1f + (ForwardBias * math.sqrt(math.abs(dotRatio))));
            }

            return hit.Distance;
        }
    }
}
