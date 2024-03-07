using Unity.Entities;

namespace Unity.NetCode.Extensions
{
    public struct EnableConnectionMonitor : IComponentData
    {
    }
    
    public struct ConnectionStateInfoRequest : IRpcCommand
    {
        public int ReasonID;
        public int State;
    }
}