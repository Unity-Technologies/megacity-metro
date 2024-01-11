using Unity.Entities;
using Unity.NetCode;

namespace Unity.MegacityMetro
{
    /// <summary>
    ///     Taken from: https://github.com/Unity-Technologies/EntityComponentSystemSamples/blob/master/NetcodeSamples/Assets/SamplesCommon.Mixed/SetRpcSystemDynamicAssemblyListSystem.cs
    ///     We use a dynamic assembly list so we can build a server with a subset of the assemblies
    ///     (only including one of the samples instead of all).
    ///     If you only have a single game in the project you generally do not need to enable DynamicAssemblyList.
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [CreateAfter(typeof(RpcSystem))]
    public partial class SetRpcSystemDynamicAssemblyListSystem : SystemBase
    {
        protected override void OnCreate()
        {
            SystemAPI.GetSingletonRW<RpcCollection>().ValueRW.DynamicAssemblyList = true;

            Enabled = false;
        }

        protected override void OnUpdate() { }
    }
}