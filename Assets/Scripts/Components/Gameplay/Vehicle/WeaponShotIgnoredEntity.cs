using Unity.Entities;

namespace Unity.MegacityMetro.Gameplay
{
    /// <summary>
    /// This component should be used for Laser to ignore collision
    /// </summary>
    public struct WeaponShotIgnoredEntity : IBufferElementData
    {
        public Entity Value;
    }
}