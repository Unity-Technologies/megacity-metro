using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;

namespace Utils.Misc
{
    public struct ClosestRaycastCollisionWithIgnoredEntitiesCollector : ICollector<RaycastHit>
    {
        public bool EarlyOutOnFirstHit => false;
        public float MaxFraction => 1f;
        public int NumHits => ClosestHit.Entity != Entity.Null ? 1 : 0;

        public RaycastHit ClosestHit;
        private NativeArray<Entity> _ignoredEntities;

        public ClosestRaycastCollisionWithIgnoredEntitiesCollector(in NativeArray<Entity> ignoredEntities)
        {
            ClosestHit = default;
            ClosestHit.Fraction = float.MaxValue;
            _ignoredEntities = ignoredEntities;
        }

        public bool AddHit(RaycastHit hit)
        {
            for (int i = 0; i < _ignoredEntities.Length; i++)
            {
                if(_ignoredEntities[i] == hit.Entity)
                {
                    return false;
                }
            }
            
            if (!PhysicsUtilities.IsCollidable(hit.Material))
            {
                return false;
            }

            // Process valid hit
            if (hit.Fraction < ClosestHit.Fraction)
            {
                ClosestHit = hit;
                return true;
            }

            return false;
        }
    }
    
    public class PhysicsUtilities
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCollidable(in Unity.Physics.Material material)
        {
            if (material.CollisionResponse == CollisionResponsePolicy.Collide ||
                material.CollisionResponse == CollisionResponsePolicy.CollideRaiseCollisionEvents)
            {
                return true;
            }

            return false;
        }
        
        public static CollisionFilter ProduceCollisionFilter(LayerMask collidesWithLayers)
        {
            uint collidesWithMask = 0u;
            collidesWithMask |= (uint)collidesWithLayers.value;
            
            CollisionFilter filter = CollisionFilter.Default;
            filter.CollidesWith = collidesWithMask;

            return filter;
        }
    }
}