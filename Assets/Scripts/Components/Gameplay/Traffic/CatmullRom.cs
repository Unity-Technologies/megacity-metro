using Unity.Mathematics;

namespace Unity.MegacityMetro.Traffic
{
    public static class CatmullRom
    {
        /// <summary>
        /// An extension utility class for multiples mathematics multiplications.
        /// </summary>
        public static float3 GetPosition(float3 p0, float3 p1, float3 p2, float3 p3, float time)
        {
            float t = time;
            float squaredT = t * t;
            float cubedT = squaredT * t;

            return 0.5f * ((2.0f * p1) + (-p0 + p2) * t + (2.0f * p0 - 5.0f * p1 + 4.0f * p2 - p3) * squaredT +
                           (-p0 + 3.0f * p1 - 3.0f * p2 + p3) * cubedT);
        }

        public static float3 GetTangent(float3 p0, float3 p1, float3 p2, float3 p3, float time)
        {
            float t = time;
            float squaredT = t * t;

            float3 deriv1 = 0.5f * ((-p0 + p2) + 2.0f * (2.0f * p0 - 5.0f * p1 + 4.0f * p2 - p3) * t +
                                    3 * (-p0 + 3 * p1 - 3 * p2 + p3) * squaredT);

            return deriv1;
        }

        public static quaternion GetOrientation(float3 p0, float3 p1, float3 p2, float3 p3, float time)
        {
            return quaternion.LookRotation(math.normalize(GetTangent(p0, p1, p2, p3, time)),
                new float3(0.0f, 1.0f, 0.0f));
        }

        public static quaternion DirectionToRotationWorldUp(float3 direction)
        {
            return quaternion.LookRotation(math.normalize(direction), new float3(0.0f, 1.0f, 0.0f));
        }

        public static float3 GetConcavity(float3 p0, float3 p1, float3 p2, float3 p3, float time)
        {
            float t = time;

            float3 deriv2 = 0.5f *
                            (2.0f * (2.0f * p0 - 5.0f * p1 + 4.0f * p2 - p3) + 6 * (-p0 + 3 * p1 - 3 * p2 + p3) * t);

            return deriv2;
        }

        public static float ComputeArcLength(float3 p0, float3 p1, float3 p2, float3 p3, int subdivs)
        {
            double dist = 0.0;

            float3 pprev = p1;

            float tstep = 1.0f / (subdivs - 1);

            float t = tstep;

            for (int i = 0; i < subdivs; ++i, t += tstep)
            {
                float3 pnext = GetPosition(p0, p1, p2, p3, t);
                dist += math.length(pnext - pprev);
                pprev = pnext;
            }

            return (float)dist;
        }
    }
}
