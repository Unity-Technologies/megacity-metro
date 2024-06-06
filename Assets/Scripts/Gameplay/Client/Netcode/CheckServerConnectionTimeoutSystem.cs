using Unity.Entities;
using Unity.MegacityMetro.UI;
using Unity.NetCode;

namespace Unity.MegacityMetro.Gameplay
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct CheckServerConnectionTimeoutSystem : ISystem
    {
        private float m_CurrentTimer;
        private bool m_ConnectionCancelled;
        
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkStreamConnection>();
        }

        public void OnUpdate(ref SystemState state)
        {
            if(m_ConnectionCancelled || MatchMakingUI.Instance == null)
                return;
            
            if (SystemAPI.TryGetSingletonRW<NetworkStreamConnection>(out var connection))
            {
                if (connection.ValueRO.CurrentState == ConnectionState.State.Connecting)
                {
                    m_CurrentTimer += SystemAPI.Time.DeltaTime;
                    if (m_CurrentTimer >= MatchMakingUI.Instance.ConnectionTimeout)
                    {
                        ModalWindow.Instance.Show("It was not possible to connect to the server.", "OK", () =>
                        {
                            GameSettingsOptionsMenu.Instance.ShowSettingsOptions(false);
                            LoadingScreen.Instance.Hide();
                            QuitSystem.DisconnectAllPlayers();
                        });
                        m_ConnectionCancelled = true;
                    }
                }
            }
        }
    }
}