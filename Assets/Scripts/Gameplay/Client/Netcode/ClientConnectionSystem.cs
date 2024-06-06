using Unity.Entities;
using Unity.MegacityMetro.UI;
using Unity.NetCode;
using UnityEngine;

namespace Unity.MegacityMetro
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct CheckServerConnectionSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            
            foreach (var (connectionState, entity) in SystemAPI.Query<RefRO<ConnectionState>>().WithEntityAccess())
            {
                if (connectionState.ValueRO.CurrentState == ConnectionState.State.Disconnected)
                {
                    Debug.Log($"[Client] Disconnected from server >> {connectionState.ValueRO.DisconnectReason}");
                    ModalWindow.Instance.Show("Disconnected from server", "OK", () =>
                    {
                        GameSettingsOptionsMenu.Instance.ShowSettingsOptions(false);
                        SceneController.LoadMenu();
                    });
                    ecb.RemoveComponent<ConnectionState>(entity);
                }
            }
        }
    }
}