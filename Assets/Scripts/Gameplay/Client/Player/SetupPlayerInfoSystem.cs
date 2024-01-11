using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Services.Samples;
using static Unity.Entities.SystemAPI;

namespace Unity.MegacityMetro.Gameplay
{
    /// <summary>
    /// Send Client information when the Connection is setup
    /// </summary>
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial struct SendPlayerInfoSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkStreamInGame>();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (PlayerInfoController.Instance == null)
                return;

            var commandBuffer = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (streaming, entity) in Query<RefRO<NetworkStreamInGame>>().WithNone<NameAssigned>().WithEntityAccess())
            {
                var requestEntity = commandBuffer.CreateEntity();
                var requestData = new SetPlayerInfoRequest
                {
                    Name = PlayerInfoController.Instance.PlayerName,
                    //This returns [True] if the requester is not a Thin Client
                    IsClient = !state.World.IsThinClient(),

                };
                if (requestData.IsClient)
                {
                    requestData.UASId = PlayerAuthentication.PlayerId;
                }
                commandBuffer.AddComponent<SendRpcCommandRequest>(requestEntity);
                commandBuffer.AddComponent(requestEntity, requestData);
                commandBuffer.AddComponent(entity, new NameAssigned());
            }

            commandBuffer.Playback(state.EntityManager);
        }
    }
}
