using UnityEngine;

namespace Unity.MegacityMetro.Gameplay
{
    /// <summary>
    /// Spawn point for the player
    /// </summary>
    public class SpawnPoint : MonoBehaviour
    {
        private Transform m_Transform;
        private void OnDrawGizmos()
        {
            if (m_Transform == null)
                m_Transform = transform;
            
            var position = m_Transform.position;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(position, 10f);
            Gizmos.DrawSphere(position, 1f);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(position, position + (m_Transform.forward * 15f) );
        }
    }
}
