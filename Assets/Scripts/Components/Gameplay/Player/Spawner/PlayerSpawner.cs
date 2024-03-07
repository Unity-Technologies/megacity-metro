using Unity.Entities;

namespace Unity.MegacityMetro.Gameplay
{
    /// <summary>
    /// Prefab to spawn for players
    /// </summary>
    public struct PlayerSpawner : IComponentData
    {
        public Entity Player;
        public Entity SinglePlayer;
    }

    public struct SinglePlayer : IComponentData { }
}
