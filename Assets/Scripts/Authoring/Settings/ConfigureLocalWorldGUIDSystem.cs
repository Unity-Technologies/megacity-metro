#if UNITY_EDITOR
using Unity.Entities;
using Unity.Entities.Build;
using Unity.Scenes;

namespace Unity.MegacityMetro.Authoring
{
    [WorldSystemFilter(WorldSystemFilterFlags.LocalSimulation)]
    [CreateAfter(typeof(SceneSystem))]
    public partial struct ConfigureLocalWorldGUIDSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            ref var sceneSystemGuid = ref state.EntityManager.GetComponentDataRW<SceneSystemData>(state.World.GetExistingSystem<SceneSystem>()).ValueRW;
            sceneSystemGuid.BuildConfigurationGUID = DotsGlobalSettings.Instance.GetClientGUID();
            state.Enabled = false;
        }
    }
}
#endif