using Unity.Collections;
using Unity.Entities;

namespace Unity.NetCode.Extensions
{
    /// <summary>
    /// Stores the player's name
    /// </summary>
    public struct PlayerName : IComponentData
    {
        [GhostField] public FixedString64Bytes Name;
    }
    
    /// <summary>
    /// Stores bot's name
    /// </summary>
    public struct BotNameElement : IBufferElementData
    {
        public FixedString64Bytes Name;
    }

    /// <summary>
    /// Allows for identification of the player's ID in the UGS service.
    /// </summary>
    public struct PlayerUASID : IComponentData
    {
        [GhostField] public FixedString64Bytes UASId;
    }

    /// <summary>
    /// Create an element to link the name and ID of the player online. 
    /// </summary>
    public struct PlayerConnectedElement : IBufferElementData
    {
        public FixedString64Bytes Name;
        public FixedString64Bytes UASId;
        public Entity Value;
    }
}
