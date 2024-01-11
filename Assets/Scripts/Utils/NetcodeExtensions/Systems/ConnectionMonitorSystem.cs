using Unity.Collections;
using Unity.Entities;

namespace Unity.NetCode.Extensions
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation, WorldSystemFilterFlags.ServerSimulation)]
    public partial struct ConnectionMonitorSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EnableConnectionMonitor>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (_, entity) in SystemAPI
                         .Query<RefRO<NetworkStreamConnection>>()
                         .WithNone<ConnectionState>()
                         .WithEntityAccess())
            {
                ecb.AddComponent<ConnectionState>(entity);
            }
            
            foreach (var (networkId, entity) in SystemAPI
                         .Query<RefRO<NetworkId>>()
                         .WithNone<InitializedConnection>()
                         .WithEntityAccess())
            {
                var connectionInfoRequest = new ConnectionStateInfoRequest
                {
                    State = (int)ConnectionState.State.Connected,
                };
                var requestEntity = ecb.CreateEntity();
                ecb.AddComponent<SendRpcCommandRequest>(requestEntity);
                ecb.AddComponent(requestEntity, connectionInfoRequest);
                ecb.AddComponent(entity, new InitializedConnection());
                UnityEngine.Debug.Log($"[Server] New connection ID:{networkId.ValueRO.Value}");
            }
            
            foreach (var (connectionState, entity) in SystemAPI
                         .Query<RefRO<ConnectionState>>()
                         .WithNone<NetworkStreamConnection>()
                         .WithEntityAccess())
            {
                var id = connectionState.ValueRO.NetworkId;
                var reasonId = (int)connectionState.ValueRO.DisconnectReason;
                var reason = DisconnectReasonEnumToString.Convert(reasonId);
                var disconnectedSuccessInfoRequest = new ConnectionStateInfoRequest
                {
                    ReasonID = reasonId,
                    State = (int)ConnectionState.State.Disconnected,
                };
                var requestEntity = ecb.CreateEntity();
                ecb.AddComponent<SendRpcCommandRequest>(requestEntity);
                ecb.AddComponent(requestEntity, disconnectedSuccessInfoRequest);
                ecb.RemoveComponent<ConnectionState>(entity);
                UnityEngine.Debug.Log($"[Server] Connection disconnected ID:{id} Reason:{reason}");
            }
            ecb.Playback(state.EntityManager);
        }
    }
}