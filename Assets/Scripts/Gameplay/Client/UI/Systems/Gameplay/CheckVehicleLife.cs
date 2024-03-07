using Unity.Entities;
using Unity.MegacityMetro.CameraManagement;
using Unity.MegacityMetro.UI;
using Unity.NetCode;

namespace Unity.MegacityMetro.Gameplay
{
    /// <summary>
    /// Check if the vehicle has died and show the death message
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct CheckVehicleLife : ISystem
    {
        private bool m_WasAlive;
        private bool m_HasDied;
        private float m_Cooldown;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GhostOwnerIsLocal>();
            state.RequireForUpdate<VehicleHealth>();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (HybridCameraManager.Instance == null)
                return;

            foreach (var (health, playerScore) in
                SystemAPI.Query<RefRO<VehicleHealth>,RefRO<PlayerScore>>()
                .WithAll<GhostOwnerIsLocal>())
            {
                if (m_WasAlive  && health.ValueRO.IsDead == 1)
                {
                    m_Cooldown = 5f;
                    m_HasDied = true;

                    // Start showing death message in UI
                    HUD.Instance.ShowDeathMessage(playerScore.ValueRO.KillerName.ToString());
                }

                m_WasAlive = health.ValueRO.IsDead == 0;
            }

            if (!m_HasDied)
                return;

            if (m_Cooldown > 0)
            {
                m_Cooldown -= SystemAPI.Time.DeltaTime;
            }
            else if(m_WasAlive)
            {
                m_HasDied = false;
                HUD.Instance.HideMessageScreen();
            }
        }
    }
}
