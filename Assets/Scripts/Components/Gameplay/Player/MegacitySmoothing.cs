using System;
using AOT;
using Unity.Assertions;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

namespace Unity.MegacityMetro.Gameplay
{
    /// <summary>
    /// The MegacityMetro-tuned prediction error <see cref="SmoothingAction"/> function for the <see cref="LocalTransform"/> component.
    /// </summary>
    [BurstCompile]
    public unsafe struct MegacityMetroSmoothingAction
    {
        /// <summary>
        /// The default value for the <see cref="DefaultSmoothingActionUserParams"/> if the no user data is passed to the function.
        /// Position is corrected if the prediction error is at least 1 unit (usually mt) and less than 10 unit (usually mt)
        /// </summary>
        public sealed class DefaultStaticUserParams
        {
            internal static readonly SharedStatic<float> maxDist = SharedStatic<float>.GetOrCreate<DefaultStaticUserParams, MaxDistKey>();
            internal static readonly SharedStatic<float> delta = SharedStatic<float>.GetOrCreate<DefaultStaticUserParams, DeltaKey>();

            static DefaultStaticUserParams()
            {
                maxDist.Data = 20;
                delta.Data = 0.5f;
            }
            class MaxDistKey {}
            class DeltaKey {}
        }

        /// <summary>
        /// Return a the burst compatible function pointer that can be used to register the smoothing action to the
        /// <see cref="GhostPredictionSmoothing"/> singleton.
        /// </summary>
        public static readonly PortableFunctionPointer<GhostPredictionSmoothing.SmoothingActionDelegate>
            Action = new(SmoothingAction);

        [BurstCompile(DisableDirectCall = true)]
        [MonoPInvokeCallback(typeof(GhostPredictionSmoothing.SmoothingActionDelegate))]
        private static void SmoothingAction(IntPtr currentData, IntPtr previousData, IntPtr usrData)
        {
            ref var trans = ref UnsafeUtility.AsRef<LocalTransform>((void*)currentData);
            ref var backup = ref UnsafeUtility.AsRef<LocalTransform>((void*)previousData);

            float maxDist = DefaultStaticUserParams.maxDist.Data;
            float delta = DefaultStaticUserParams.delta.Data;

            Assert.IsTrue(usrData.ToPointer() == null);

            var dist = math.distance(trans.Position, backup.Position);
            if (dist < maxDist && dist > delta && dist > 0)
            {
                trans.Position = backup.Position + (trans.Position - backup.Position) * delta / dist;
            }
        }
    }
}