using Unity.Entities;
using Unity.MegacityMetro.Gameplay;
using Unity.NetCode;
using static Unity.Entities.SystemAPI;

namespace Unity.MegacityMetro.UI
{
    /// <summary>
    /// Update the notification UI
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct UpdateNotification : ISystem
    {
        private EntityQuery m_LocalPlayerQuery;
        private int m_CurrentKills;
        
        public void OnCreate(ref SystemState state)
        {
            m_LocalPlayerQuery = state.GetEntityQuery(ComponentType.ReadOnly<GhostOwnerIsLocal>());
            state.RequireForUpdate(m_LocalPlayerQuery);
            m_CurrentKills = 0;
        }

        public void OnUpdate(ref SystemState state)
        {
            if (HUD.Instance == null)
                return;

            foreach (var playerAspect in Query<PlayerScoreAspect>().WithAll<GhostOwnerIsLocal>())
            {
                if(playerAspect.Kills > m_CurrentKills)
                {
                    m_CurrentKills = playerAspect.Kills;
                    var message = $"<color=#DF3D06>YOU DESTROYED</color>\n{playerAspect.Killed}";
                    HUD.Instance.Notification.Message(message);
                }
            }
        }
    }
}
