
using Unity.Entities;

namespace Unity.MegacityMetro
{
    [WorldSystemFilter(WorldSystemFilterFlags.LocalSimulation)]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct InitLocalRateSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            RateSettings.ApplyRates(state.World.GetOrCreateSystemManaged<FixedStepSimulationSystemGroup>());
        }
    }
}