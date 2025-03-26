#if UNITY_SERVER
using Unity.Collections;
using Unity.Entities;
using Unity.MegacityMetro.Gameplay;
using Unity.Services.MultiplayerSDK.Server;

namespace Gameplay.Server.Netcode
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateAfter(typeof(VerifyPlayerDisconnectedSystem))]
    public partial class PlayerLeftEventSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            // Wait for the session manager to be initialized to consume player left events
            if (ServerBehaviour.Instance?.ServerManager?.SessionEventManager == null)
                return;
            
            var ecb = new EntityCommandBuffer(Allocator.TempJob);

            Entities.WithAll<PlayerLeftEvent>().ForEach((Entity entity, ref PlayerLeftEvent playerLeftEvent) =>
            {
                UnityEngine.Debug.Log($"Player {playerLeftEvent.PlayerId} has left the game.");
                ServerBehaviour.Instance.ServerManager.SessionEventManager.RemovePlayer(playerLeftEvent.PlayerId.ToString());
                ecb.DestroyEntity(entity);
            }).WithoutBurst().Run();
            
            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}
#endif