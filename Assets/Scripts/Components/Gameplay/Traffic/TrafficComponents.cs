using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.Splines;

namespace Unity.MegacityMetro.Traffic
{
    /// <summary>
    /// NPC vehicles and traffic specific components
    /// </summary>
    public static class Constants
    {
        public const float NodePositionRounding = 100.0f; // 100 = 2dp, 1 = 0dp (a meter), .1 = (10 meters) etc.

        public const int
            RoadOccupationSlotsMax =
                16; // Number of slots in a road segment (for a lane length) (so VehicleLength * this = longest possible road section)

        public const int RoadLanes = 3; // Number of lanes (in a line)
        public const int RoadIndexMultiplier = RoadLanes * RoadOccupationSlotsMax;
        public const float VehicleLength = 30.0f;
        public const float VehicleWidth = 10.0f;

        public const float brakeFactor = 1.0f / (5.0f * 60.0f);
        public const float accelFactor = 1.0f / (20.0f * 60.0f);

        public const float VehicleSpeedMin = 0.2f;
        public const float VehicleSpeedMax = 1.0f;
        public const float VehicleSpeedFudge = 100.0f; // Compensate for dt-based updates
        public const byte LaneSwitchDelay = 99;

        public const float AvoidanceRadius = 4.0f;
        public const float AvoidanceRadiusPlayer = 24.0f;
        public const float MaxTetherSquared = 2500f;

        public const float SlowingDistanceMeters = 45.0f;
        public const float MaxSpeedMetersPerSecond = 10.0f;
    }
    
    public struct Road : IComponentData 
    {
        public NativeSpline Spline;
        public float Length;
    }
    
    public struct VehiclePathing : IComponentData
    {
        public float SplinePos;
        public int RoadIndex;
        public uint RandomSeed;
        // public Random Random;
    }

    public struct TrafficConfig : IComponentData
    {
        public float GlobalCarSpeed;  // meters per second
        public int MaxVehiclesPerRoad;
        public int MaxVehiclesTotal;
        public float LaneOffsetScale;
        public float BankingMax;
        public float MinDistanceBetweenVehicles;
        public float MaxDistanceBetweenVehicles;
    }
    
    public struct VehiclePrefabRef : IComponentData
    {
        public Entity VehiclePrefab;
        public float VehicleSpeed;
    }
    
    public struct RoadPoint : IBufferElementData
    {
        public float3 Value;
    }

    public struct RoadOffset : IComponentData
    {
        public float3 Value;
    }
    
    public struct TrafficEnabled : IComponentData
    {
    }

    public struct RoadSection : IComponentData
    {
        public int sortIndex;

        // Add lane widths
        public float width;
        public float height;

        // Add lane speeds
        public float minSpeed;
        public float maxSpeed;

        // Cubic segment
        public float3 p0;
        public float3 p1;
        public float3 p2;
        public float3 p3;

        // Arc length between p1 - p2
        public float arcLength;

        public float vehicleHalfLen;
        public float linkExtraChance;

        public int occupationLimit;
        public int linkExtra;
        public int linkNext;
    }

    public struct Spawner : IComponentData
    {
        public float3 Direction;
        public float3 Position;
        public float Time;

        public float minSpeed;
        public float maxSpeed;

        public int delaySpawn;
        public int RoadIndex;
        public int LaneIndex; // see VehiclePathing
        public uint poolSpawn;

        public Random random;
    }

    public struct RoadSectionBlobRef : IComponentData
    {
        public BlobAssetReference<RoadSectionBlob> Data;
    }

    public struct RoadSectionBlob
    {
        public BlobArray<RoadSection> RoadSections;
    }
}
