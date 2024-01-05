using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

namespace Unity.MegacityMetro.Utils
{
    [MaterialProperty("_Color_MatChannel_Blue")]
    public struct LayeredLitBaseColor3 : IComponentData
    {
        public float4 Value;
    }
}
