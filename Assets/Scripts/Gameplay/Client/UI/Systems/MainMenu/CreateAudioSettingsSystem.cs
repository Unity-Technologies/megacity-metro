using Unity.Entities;
using Unity.MegacityMetro.Audio;

namespace Unity.MegacityMetro.Gameplay
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.LocalSimulation)]
    public partial struct CreateAudioSettingsSystem : ISystem
    {
        private EntityQuery m_AudioSettingsQuery;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerSpawner>();
            m_AudioSettingsQuery = state.GetEntityQuery(ComponentType.ReadOnly<AudioSystemSettings>());
        }

        public void OnUpdate(ref SystemState state)
        {
            if (m_AudioSettingsQuery.CalculateEntityCount() > 0)
                return;
            
            var audioSystemEntity = state.EntityManager.CreateEntity(typeof(AudioSystemSettings));
            var audioMaster = AudioMaster.Instance;
            var systemSettings = new AudioSystemSettings
            {
                DebugMode = audioMaster.showDebugLines,
                MaxDistance = audioMaster.maxDistance,
                MaxVehicles = audioMaster.maxVehicles,
                MaxSqDistance = audioMaster.maxDistance * audioMaster.maxDistance,
                ClosestEmitterPerClipCount = audioMaster.closestEmitterPerClipCount,
            };
            state.EntityManager.SetComponentData(audioSystemEntity, systemSettings);
        }
    }
}