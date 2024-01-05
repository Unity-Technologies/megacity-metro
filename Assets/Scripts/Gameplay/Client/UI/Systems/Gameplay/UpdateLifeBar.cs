using Unity.Entities;
using Unity.MegacityMetro.CameraManagement;
using Unity.MegacityMetro.Gameplay;
using Unity.NetCode;

namespace Unity.MegacityMetro.UI
{
    /// <summary>
    /// Update the life bar UI
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    [UpdateInGroup(typeof(PresentationSystemGroup), OrderLast = true)]
    public partial struct UpdateLifeBar : ISystem
    {
        private float m_PreviousLife;
        private EntityQuery m_LocalPlayerQuery;
        private bool m_CanShakeTheCamera;
        private double m_ReceivingDamageTime;
        private double m_StartShakingCameraTime;

        public void OnCreate(ref SystemState state)
        {
            m_LocalPlayerQuery = state.GetEntityQuery(ComponentType.ReadWrite<VehicleHealth>(),
                                                      ComponentType.ReadOnly<GhostOwnerIsLocal>());
            state.RequireForUpdate(m_LocalPlayerQuery);
            m_CanShakeTheCamera = true;
            m_PreviousLife = default;
        }

        public void OnUpdate(ref SystemState state)
        {
            if (HUD.Instance == null)
                return;

            const float delayForShaking = 1.2f;
            const float relaxingDamage = 0.25f;
            var currentTime = state.World.Time.ElapsedTime;
            foreach (var vehicleHealth in SystemAPI.Query<VehicleHealth>().WithAll<GhostOwnerIsLocal>())
            {
                var currentLife = vehicleHealth.Value;

                //set life the first time
                if (m_PreviousLife.Equals(default))
                {
                    m_PreviousLife = currentLife;    
                    HUD.Instance.UpdateLife(currentLife, vehicleHealth.LookAtEnemyDegrees);
                }
                
                if (m_PreviousLife != currentLife)
                {
                    // force the start receiving damage to be called only once when receiving the first hit.
                    if (m_CanShakeTheCamera && m_PreviousLife > currentLife)
                    {
                        m_StartShakingCameraTime = currentTime;
                        HybridCameraManager.Instance.StartShaking();
                        m_CanShakeTheCamera = false;
                    }
                    
                    m_ReceivingDamageTime = currentTime;
                    HUD.Instance.UpdateLife(currentLife, vehicleHealth.LookAtEnemyDegrees);
                    m_PreviousLife = currentLife;
                }
                else if(m_ReceivingDamageTime + relaxingDamage < currentTime)
                {
                    HUD.Instance.DisableDamageIndicator(currentLife);
                }
                
                if(m_StartShakingCameraTime + delayForShaking < currentTime && !m_CanShakeTheCamera)
                {
                    m_CanShakeTheCamera = true;
                }
            }
        }
    }
}
