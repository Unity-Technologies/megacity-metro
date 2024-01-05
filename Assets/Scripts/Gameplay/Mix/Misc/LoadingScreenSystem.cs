using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Scenes;
using static Unity.Entities.SystemAPI;

namespace Unity.MegacityMetro.Streaming
{
    /// <summary>
    /// Update the GameLoadInfo singleton with loaded sections number
    /// </summary>
    [BurstCompile]
    public partial struct LoadingScreenSystem : ISystem
    {
        private EntityQuery m_SceneSections;

        public void OnCreate(ref SystemState state)
        {
            // Query only scene sections that are requested to load or loaded
            m_SceneSections = state.GetEntityQuery(
                ComponentType.ReadOnly<SceneSectionData>(),
                ComponentType.ReadOnly<RequestSceneLoaded>());
            state.EntityManager.CreateEntity(typeof(GameLoadInfo));
            state.RequireForUpdate<GameLoadInfo>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var sceneSectionEntities = m_SceneSections.ToEntityArray(Allocator.TempJob);
            var loadedSceneSections = 0;
            foreach (var entity in sceneSectionEntities)
            {
                if (SceneSystem.IsSectionLoaded(state.WorldUnmanaged, entity))
                {
                    loadedSceneSections++;
                }
            }

            var gameLoadInfo = new GameLoadInfo
            {
                TotalSceneSections = sceneSectionEntities.Length,
                LoadedSceneSections = loadedSceneSections
            };

            SetSingleton(gameLoadInfo);
            sceneSectionEntities.Dispose(state.Dependency);
        }
    }
}
