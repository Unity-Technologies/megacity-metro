using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine.Scripting;

namespace Unity.MegacityMetro.Gameplay
{
    /// <summary>
    /// Variants for the MegacityMetro project
    /// </summary>
    [BakingVersion("megacity-metro", 2)]
    public sealed partial class MegacityMetroVariants : DefaultVariantSystemBase
    {
        protected override void RegisterDefaultVariants(Dictionary<ComponentType, Rule> defaultVariants)
        {
            defaultVariants.Add(ComponentType.ReadOnly<LocalTransform>(), Rule.OnlyParents(typeof(MegacityMetroLocalTransform)));
            defaultVariants.Add(ComponentType.ReadOnly<PhysicsVelocity>(), Rule.OnlyParents(typeof(MegacityMetroPhysicsVelocity)));
        }
    }

    [Preserve]
    [GhostComponentVariation(typeof(LocalTransform), "MegacityMetro - LocalTransform")]
    [GhostComponent(PrefabType = GhostPrefabType.All, SendTypeOptimization = GhostSendType.AllClients, SendDataForChildEntity = false)]
    public struct MegacityMetroLocalTransform
    {
        [GhostField(Quantization = 1000, Smoothing = SmoothingAction.InterpolateAndExtrapolate, MaxSmoothingDistance = 20)]
        public float3 Position;
    }

    [GhostComponentVariation(typeof(PhysicsVelocity), "MegacityMetro - PhysicsVelocity")]
    [GhostComponent(PrefabType = GhostPrefabType.All, SendTypeOptimization = GhostSendType.OnlyPredictedClients)]
    public struct MegacityMetroPhysicsVelocity
    {
        [GhostField(Quantization = 1000)]
        public float3 Linear;
    }
}
