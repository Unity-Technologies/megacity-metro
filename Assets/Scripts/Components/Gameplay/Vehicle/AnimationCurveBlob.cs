using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Unity.MegacityMetro.Gameplay
{
    public struct AnimationCurveBlob
    {
        private BlobArray<float> Keys;
        private float Length;
        private float KeyCount;

        // When t exceeds the curve time, repeat it
        public float CalculateNormalizedTime(float t)
        {
            float normalizedT = t * Length;
            return normalizedT - math.floor(normalizedT);
        }

        public float Evaluate(float t)
        {
            // Loops time value between 0...1
            t = CalculateNormalizedTime(t);

            // Find index and interpolation value in the array
            float sampleT = t * KeyCount;
            var sampleTFloor = math.floor(sampleT);

            float interp = sampleT - sampleTFloor;
            var index = (int) sampleTFloor;

            return math.lerp(Keys[index], Keys[index + 1], interp);
        }

        public static BlobAssetReference<AnimationCurveBlob> CreateBlob(AnimationCurve curve, Allocator allocator,
            Allocator allocatorForTemp = Allocator.TempJob)
        {
            using (var blob = new BlobBuilder(allocatorForTemp))
            {
                ref var anim = ref blob.ConstructRoot<AnimationCurveBlob>();
                int keyCount = 20;

                var endTime = curve[curve.length - 1].time;
                anim.Length = 1.0F / endTime;
                anim.KeyCount = keyCount;

                var array = blob.Allocate(ref anim.Keys, keyCount + 1);
                for (int i = 0; i < keyCount; i++)
                {
                    var t = (float) i / (keyCount - 1) * endTime;
                    array[i] = curve.Evaluate(t);
                }

                array[keyCount] = array[keyCount - 1];

                return blob.CreateBlobAssetReference<AnimationCurveBlob>(allocator);
            }
        }
    }
}