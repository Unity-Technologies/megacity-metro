using Unity.Collections;
using Unity.NetCode;

namespace Unity.MegacityMetro.Gameplay
{
    public struct  SetPlayerInfoRequest : IRpcCommand
    {
        public FixedString64Bytes Name;
        public FixedString64Bytes UASId;
        public bool IsClient;
    }
}