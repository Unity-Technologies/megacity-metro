using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

namespace Unity.MegacityMetro.Utils
{
    public static class PlayerColor
    {
        public static float4 Get(int networkId)
        {
            var colorIndex = networkId % 16;
            return colorIndex switch
            {
                1 => new float4(0.42f, 0.02f, 0.05f, 1f), // Red
                2 => new float4(0.15f, 0.2f, 0.4f, 1f), // Blue
                3 => new float4(0f, 0.5f, 0.2f, 1f), // Green
                4 => new float4(1f, 0.6f, 0f, 1f), // Yellow
                5 => new float4(0.7f, 0.25f, 1f, 1f), // Purple
                6 => new float4(0.2f, 0.2f, 0.3f, 1f), // Dark-Blue
                7 => new float4(0.7f, 0.7f, 0.7f, 1f), // Light-Gray
                8 => new float4(1f, 0.25f, 0f, 1f), // Orange
                9 => new float4(0.25f, 0.85f, 1f, 1f), // Light-Blue
                10 => new float4(0.4f, 0f, 0.1f, 1f), // Purple-Red
                11 => new float4(0f, 0.5f, 0.5f, 1f), // Aquamarine
                12 => new float4(0.9f, 0.3f, 0.8f, 1f), // Magenta
                13 => new float4(0.7f, 0.2f, 0f, 1f), // Brown
                14 => new float4(0f, 0.2f, 0f, 1f), // Dark-Green
                15 => new float4(1f, 1f, 1f, 1f), // White
                16 => new float4(0.1f, 0.1f, 0.1f, 1f), // Dark-Gray
                _ => new float4(0.5f, 0.5f, 0.5f, 1f) // Gray
            };
        }
    }
}
